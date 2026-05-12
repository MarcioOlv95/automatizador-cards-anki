using automatizador_cards_anki.api.application.Cards.Messaging.Requests;
using automatizador_cards_anki.api.domain.Entities;
using automatizador_cards_anki.api.domain.Helper;
using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;
using automatizador_cards_anki.api.domain.Shared;
using automatizador_cards_anki.api.domain.Shared.Interface;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace automatizador_cards_anki.api.application.Cards.Handlers;

public class InsertCardsHandler : IRequestHandler<InsertCardsRequest, Result>
{
    private readonly IOpenAiApiManager _openAiApiManager;
    private readonly IAnkiApiManager _ankiApiManager;
    private readonly IConfiguration _configuration;
    private readonly IImageService _imageService;
    private readonly ILogger<InsertCardsHandler> _logger;

    private readonly string DECK_NAME;
    private const int ANKI_VERSION = 6;
    private const string QUESTION_CHAT_MEANING_PHRASES =
        "Give me the meaning and one simple phrase with the word: {0}.";
    private const string QUESTION_CHAT_MEANING_IMAGE = "Give me a image that describe the meaning of the word: {0}";
    private const string FOLDER_NAME = "images";
    private const int MAX_CONCURRENT_REQUESTS = 3;

    public InsertCardsHandler(
        IOpenAiApiManager openAiApiManager,
        IAnkiApiManager ankiApiManager,
        IConfiguration configuration,
        ILogger<InsertCardsHandler> logger,
        IImageService imageService)
    {
        _configuration = configuration;
        _openAiApiManager = openAiApiManager;
        _ankiApiManager = ankiApiManager;
        DECK_NAME = _configuration.GetValue<string>("DeckName")!;
        _logger = logger;
        _imageService = imageService;
    }

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
            _logger.LogError($"There was an error. Message: {ex.Message}. InnerException: {ex.InnerException ?? ex.InnerException}. StackTrace: {ex.StackTrace}");
            return Result.Failure(ex.Message);
        }
        finally
        {
            await RemoveFilesAsync();
        }
    }

    private async Task<List<CardAnki>> GetCardAnkiAsync(InsertCardsRequest request, CancellationToken cancellationToken)
    {
        var words = request.Words ?? new List<string>();
        var results = new List<CardAnki>();

        using var semaphore = new SemaphoreSlim(MAX_CONCURRENT_REQUESTS);
        var tasks = words.Select(async word =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var prompt = string.Format(QUESTION_CHAT_MEANING_PHRASES, word);
                var answer = await _openAiApiManager.CreateConversationAsync(prompt, cancellationToken).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(answer))
                {
                    _logger.LogWarning("OpenAI returned empty answer for word {Word}", word);
                    return (CardAnki?)null;
                }

                var cleaned = answer.Replace("\\n", string.Empty)
                                    .Replace("\n", string.Empty)
                                    .Replace("\"", string.Empty)
                                    .Replace(@"\", string.Empty)
                                    .Trim();

                var phraseMarker = "phrase:";
                var idx = cleaned.IndexOf(phraseMarker, StringComparison.OrdinalIgnoreCase);
                string meaningPart;
                string phrasePart;

                if (idx >= 0)
                {
                    meaningPart = cleaned.Substring(0, idx).Trim();
                    phrasePart = cleaned.Substring(idx + phraseMarker.Length).Trim();
                }
                else
                {
                    var firstPeriod = cleaned.IndexOf('.');
                    if (firstPeriod > 0)
                    {
                        meaningPart = cleaned.Substring(0, firstPeriod + 1).Trim();
                        phrasePart = cleaned.Substring(firstPeriod + 1).Trim();
                    }
                    else
                    {
                        meaningPart = cleaned;
                        phrasePart = string.Empty;
                    }
                }

                meaningPart = meaningPart.ToLowerInvariant();
                meaningPart = meaningPart.Replace("meaning:", string.Empty)
                                        .Replace("simple", string.Empty)
                                        .Trim();

                var image = await _openAiApiManager.GenerateImageAsync(string.Format(QUESTION_CHAT_MEANING_IMAGE, word)).ConfigureAwait(false);

                var urlImage = string.Empty;
                try
                {
                    urlImage = await _imageService.ResizeImageAsync(image, word).ConfigureAwait(false) ?? string.Empty;
                }
                catch (Exception imgEx)
                {
                    _logger.LogWarning(imgEx, "Image resizing failed for {Word}: {Msg}", word, imgEx.Message);
                }

                var phraseFormatted = StringHelper.ToFirstLetterUpperCase(phrasePart.Replace(word, $"<b>{word}</b>"));
                var meaningFormatted = StringHelper.ToFirstLetterUpperCase(meaningPart);

                var card = new CardAnki(word,
                    phraseFormatted,
                    meaningFormatted,
                    urlImage);

                return (CardAnki?)card;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing word {Word}: {Message}", word, ex.Message);
                return (CardAnki?)null;
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

        return await _ankiApiManager.RequestAnkiAsync(noteRequestDto, cancellationToken);
    }

    private async Task<AnkiResponse> SyncAnkiWebAsync(CancellationToken cancellationToken)
    {
        var request = new SyncAnkiWebRequestDto("sync", ANKI_VERSION);

        return await _ankiApiManager.RequestAnkiAsync(request, cancellationToken);
    }

    private async Task RemoveFilesAsync()
    {
        var pathImages = Path.Combine(Directory.GetCurrentDirectory(), FOLDER_NAME);

        if (Directory.Exists(pathImages))
            Directory.Delete(pathImages, true);
    }
}
