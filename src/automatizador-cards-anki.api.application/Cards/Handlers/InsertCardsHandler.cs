using automatizador_cards_anki.api.application.Cards.Messaging.Requests;
using automatizador_cards_anki.api.domain.Entities;
using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;
using automatizador_cards_anki.api.domain.Shared;
using automatizador_cards_anki.api.domain.Shared.Interface;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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
            await Task.WhenAll(
                AddNotesToAnkiAsync
                (GetNotesToAnkiAsync(request, cancellationToken).Result, cancellationToken), 
                SyncAnkiWebAsync(cancellationToken));

            await RemoveFilesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"There was an error. Message: {ex.Message}. InnerException: {ex.InnerException ?? ex.InnerException}. StackTrace: {ex.StackTrace}");
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
                                        await _imageService.ResizeImageAsync(image, word));

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

    private async Task<AnkiResponse> SyncAnkiWebAsync(CancellationToken cancellationToken)
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

    private async Task RemoveFilesAsync()
    {
        var pathImages = Path.Combine(Directory.GetCurrentDirectory()!.ToString(), FOLDER_NAME);

        if (Directory.Exists(pathImages))
            Directory.Delete(pathImages, true);
    }
}
