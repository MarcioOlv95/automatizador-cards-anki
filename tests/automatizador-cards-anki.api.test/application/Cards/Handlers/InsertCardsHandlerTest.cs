using AutoFixture;
using automatizador_cards_anki.api.application.Cards.Handlers;
using automatizador_cards_anki.api.application.Cards.Messaging.Requests;
using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;
using automatizador_cards_anki.api.domain.Shared;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace automatizador_cards_anki.api.test.application.Cards.Handlers;

public class InsertCardsHandlerTest
{
    private readonly Mock<IOpenAiApiManager> _openAiApiManager = new();
    private readonly Mock<IAnkiApiManager> _ankiApiManager = new();
    private readonly InsertCardsHandler _insertCardsHandler;
    private const string QUESTION_CHAT_MEANING_PHRASES =
        "Give me the meaning and one simple phrase with the word: {0}.";
    private const string QUESTION_CHAT_MEANING_IMAGE = "Give me a image that describe the meaning of the word: {0}";
    private readonly Fixture _fixture = new();

    public InsertCardsHandlerTest()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"DeckName", "DeckTeste"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _insertCardsHandler = new InsertCardsHandler(_openAiApiManager.Object, _ankiApiManager.Object, configuration);
    }

    [Fact]
    public async Task InsertCards_SucessAsync()
    {
        var request = _fixture.Create<InsertCardsRequest>();

        foreach (var word in request.Words)
        {
            var response = $"meaning: {word}. phrase: {word}";
            var responseUrlImage = _fixture.Create<string>();

            _openAiApiManager.Setup(x => x.CreateConversationAsync(string.Format(QUESTION_CHAT_MEANING_PHRASES, word), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            _openAiApiManager.Setup(x => x.GenerateImageAsync(string.Format(QUESTION_CHAT_MEANING_IMAGE, word)))
                .ReturnsAsync(responseUrlImage);
        }

        var noteRequestDto = _fixture.Create<AddNoteRequestDto>();
        _ankiApiManager.Setup(x => x.RequestAnkiAsync(noteRequestDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<AnkiResponse>());

        var syncAnkiWebRequest = _fixture.Create<SyncAnkiWebRequestDto>();
        _ankiApiManager.Setup(x => x.RequestAnkiAsync(syncAnkiWebRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<AnkiResponse>());

        var result = await _insertCardsHandler.Handle(request, It.IsAny<CancellationToken>());

        _openAiApiManager.Verify();
        _ankiApiManager.Verify();
        result.ShouldBeOfType<Result>();
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task InsertCards_ErrorAsync()
    {
        var request = _fixture.Create<InsertCardsRequest>();

        foreach (var word in request.Words)
        {
            _openAiApiManager.Setup(x => x.CreateConversationAsync(string.Format(QUESTION_CHAT_MEANING_PHRASES, word), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
        }

        var result = await _insertCardsHandler.Handle(request, It.IsAny<CancellationToken>());

        result.ShouldBeOfType<Result>();
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }
}
