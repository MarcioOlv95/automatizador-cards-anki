using automatizador_cards_anki.api.application.Cards.Messaging.Requests;
using automatizador_cards_anki.api.domain.Entities;
using automatizador_cards_anki.api.domain.Helper;
using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi.Interface;
using automatizador_cards_anki.api.domain.Shared;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace automatizador_cards_anki.api.application.Cards.Handlers;

public class InsertCardsHandler(
    IOpenAiApiManager openAiApiManager,
    IAnkiApiManager ankiApiManager,
    IConfiguration configuration,
    ILogger<InsertCardsHandler> logger
) : IRequestHandler<InsertCardsRequest, Result>
{
    private readonly string DECK_NAME = configuration.GetValue<string>("DeckName")!;
    private const int ANKI_VERSION = 6;
    private const string QUESTION_CHAT_MEANING_PHRASES = "Give me the meaning and one phrase with the word: {0}.";
    private const string QUESTION_CHAT_MEANING_IMAGE = "Give me a image that describe the meaning of the word: {0}";
    private const int MAX_CONCURRENT_REQUESTS = 5;

    public async Task<Result> Handle(InsertCardsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var notes = await GetCardAnkiAsync(request, cancellationToken);

            var addNotesTask = AddNotesToAnkiAsync(notes, cancellationToken);
            var syncTask = SyncAnkiWebAsync(cancellationToken);

            await Task.WhenAll(addNotesTask, syncTask);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError("There was an error. Message: {message}. InnerException: {innerException}. StackTrace: {stackTrace}", ex.Message, ex.InnerException, ex.StackTrace);
            return Result.Failure(ex.Message);
        }
        finally
        {
            await ImageHelper.RemoveFilesAsync();
        }
    }

    private async Task<List<CardAnki>> GetCardAnkiAsync(InsertCardsRequest request, CancellationToken cancellationToken)
    {
        var words = request.Words ?? [];
        var results = new List<CardAnki>();

        using var semaphore = new SemaphoreSlim(MAX_CONCURRENT_REQUESTS);
        var tasks = words.Select(async word =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var prompt = string.Format(QUESTION_CHAT_MEANING_PHRASES, word);
                var answer = await openAiApiManager.CreateConversationAsync(prompt, cancellationToken).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(answer.Text))
                {
                    logger.LogWarning("OpenAI returned empty answer for word {Word}", word);
                    return (CardAnki?)null;
                }

                answer.CleanText();
                answer.GetMeaningPart();
                answer.GetPhrasePart();
                answer.FormatMeaning();
                answer.FormatPhrase(word);

                var urlImage = await openAiApiManager.GenerateImageUriAsync(string.Format(QUESTION_CHAT_MEANING_IMAGE, word), word, cancellationToken)
                    .ConfigureAwait(false);

                var card = new CardAnki(word,
                    answer.PhrasePart,
                    answer.MeaningPart,
                    urlImage);

                return card;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing word {Word}: {Message}", word, ex.Message);
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        var completed = await Task.WhenAll(tasks).ConfigureAwait(false);
        results.AddRange(completed.Where(r => r is not null).Select(r => r!));
        return results;
    }

    private async Task<AnkiResponse> AddNotesToAnkiAsync(List<CardAnki> notes, CancellationToken cancellationToken)
    {
        var noteRequestDto = new AddNoteRequestDto("addNotes", ANKI_VERSION);

        var notesToParams = new List<Note>();

        foreach (var item in notes)
        {
            var front = StringHelper.ToFirstLetterUpperCase(item.Phrase.Replace(item.Word, $"<b>{item.Word}</b>"));
            var back = $"<b>{item.Word}</b><br>{StringHelper.ToFirstLetterUpperCase(item.Meaning)}<br>";

            var fields = new Field(front, back);

            var note = new Note(DECK_NAME, "Basic", fields);
            note.options.allowDuplicate = true;

            if (!string.IsNullOrEmpty(item.UrlImage))
            {
                note.picture.Add(new(item.UrlImage, item.Word + ".png", ["Back"]));
            }

            notesToParams.Add(note);
        }

        noteRequestDto.@params.notes = notesToParams;

        return await ankiApiManager.RequestAnkiAsync(noteRequestDto, cancellationToken);
    }

    private async Task<AnkiResponse> SyncAnkiWebAsync(CancellationToken cancellationToken)
    {
        var request = new SyncAnkiWebRequestDto("sync", ANKI_VERSION);

        return await ankiApiManager.RequestAnkiAsync(request, cancellationToken);
    }
}
