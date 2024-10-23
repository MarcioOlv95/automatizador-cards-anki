using automatizador_cards_anki.api.application.Cards.Messaging.Requests;
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
        "Give me the meaning and one simple phrase with the word {0}.";

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

    private async Task<List<(string, string, string)>> GetNotesToAnkiAsync(InsertCardsRequest request, CancellationToken cancellationToken)
    {
        List<(string, string, string)> inputAnki = [];

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

            var card =
            (
                word,
                answerChatGpt.Substring(0, answerChatGpt.IndexOf("phrase:")).Trim(),
                answerChatGpt.Substring(answerChatGpt.IndexOf("phrase:") + "phrase:".Length).Trim()
            );

            inputAnki.Add(card);
        }

        return inputAnki;
    }

    private async Task<AnkiResponse> AddNotesToAnkiAsync(List<(string, string, string)> notes, CancellationToken cancellationToken)
    {
        var noteRequestDto = new AddNoteRequestDto("addNotes", ANKI_VERSION);

        var notesToParams = new List<Note>();

        foreach (var item in notes)
        {
            var fields = new Field(ToFirstLetterUpperCase(item.Item3.Replace(item.Item1, $"<b>{item.Item1}</b>")), 
                            $"<b>{item.Item1}</b><br>{ToFirstLetterUpperCase(item.Item2)}");

            var note = new Note(DECK_NAME, "Basic", fields);
            note.options.allowDuplicate = true;

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
