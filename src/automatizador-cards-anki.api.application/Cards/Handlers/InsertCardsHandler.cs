using automatizador_cards_anki.api.application.Cards.Messaging.Requests;
using automatizador_cards_anki.api.domain.Entities;
using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;
using automatizador_cards_anki.api.domain.Shared;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace automatizador_cards_anki.api.application.Cards.Handlers;

public class InsertCardsHandler : IRequestHandler<InsertCardsRequest, Result>
{
    private readonly IOpenAiApiManager _openAiApiManager;
    private readonly IAnkiApiManager _ankiApiManager;
    private readonly IConfiguration _configuration;

    private readonly string DECK_NAME;
    private const int ANKI_VERSION = 6;
    private const string QUESTION_CHAT_MEANING_PHRASES =  
        "Give me the meaning and one simple phrase with the word: {0}.";
    private const string QUESTION_CHAT_MEANING_IMAGE = "Give me a image that describe the meaning of the word: {0}";

    public InsertCardsHandler(IOpenAiApiManager openAiApiManager, IAnkiApiManager ankiApiManager, IConfiguration configuration)
    {
        _configuration = configuration;
        _openAiApiManager = openAiApiManager;
        _ankiApiManager = ankiApiManager;
        DECK_NAME = _configuration.GetValue<string>("DeckName")!;
    }

    public async Task<Result> Handle(InsertCardsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var notes = await GetNotesToAnkiAsync(request, cancellationToken);

            await AddNotesToAnkiAsync(notes, cancellationToken);

            await SyncAnkiWeb(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<List<CardAnki>> GetNotesToAnkiAsync(InsertCardsRequest request, CancellationToken cancellationToken)
    {
        List<CardAnki> cardsAnki = [];

        foreach (var word in request.Words)
        {
            var answerChatGpt = await _openAiApiManager.CreateConversationAsync(string.Format(QUESTION_CHAT_MEANING_PHRASES, word), cancellationToken);

            answerChatGpt = answerChatGpt.Replace("\\n", string.Empty)
                                        .Replace("\n", " ")
                                        .Replace("\"", string.Empty)
                                        .Replace(@"\", string.Empty)
                                        .ToLowerInvariant()
                                        .Replace("meaning:", string.Empty)
                                        .Replace("simple", string.Empty);

            var image = await _openAiApiManager.GenerateImageAsync(string.Format(QUESTION_CHAT_MEANING_IMAGE, word));

            var cardAnki = new CardAnki(word, 
                                        answerChatGpt.Substring(answerChatGpt.IndexOf("phrase:") + "phrase:".Length).Trim(),
                                        answerChatGpt.Substring(0, answerChatGpt.IndexOf("phrase:")).Trim(),
                                        image);

            cardsAnki.Add(cardAnki);
        }

        return cardsAnki;
    }

    private async Task<AnkiResponse> AddNotesToAnkiAsync(List<CardAnki> notes, CancellationToken cancellationToken)
    {
        var noteRequestDto = new AddNoteRequestDto("addNotes", ANKI_VERSION);

        var notesToParams = new List<Note>();

        foreach (var item in notes)
        {
            var fields = new Field(ToFirstLetterUpperCase(item.Phrase.Replace(item.Word, $"<b>{item.Word}</b>")), 
                            $"<b>{item.Word}</b><br>{ToFirstLetterUpperCase(item.Meaning)}<br>");

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

    private async Task<AnkiResponse> SyncAnkiWeb(CancellationToken cancellationToken)
    {
        var request = new SyncAnkiWebRequestDto("sync", ANKI_VERSION);

        return await _ankiApiManager.RequestAnkiAsync(request, cancellationToken);
    }

    private static string ToFirstLetterUpperCase(string input)
    {
        if (string.IsNullOrEmpty(input)) 
            return input;

        return $"{input[0].ToString().ToUpper()}{input.Substring(1)}";
    }
}
