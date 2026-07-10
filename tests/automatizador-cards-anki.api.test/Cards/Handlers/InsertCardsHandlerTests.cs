using System.Text.Json.Nodes;
using AutomatizadorCardsAnki = automatizador_cards_anki.api.application.Cards.Handlers;
using automatizador_cards_anki.api.application.Cards.Messaging.Requests;
using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace automatizador_cards_anki.api.test.Cards.Handlers
{
    public class InsertCardsHandlerTests
    {
        [Fact]
        public async Task Handle_RequestSuccessful_ReturnsSuccess()
        {
            // Arrange
            var openAiMock = new Mock<IOpenAiApiManager>();
            var ankiMock = new Mock<IAnkiApiManager>();
            var loggerMock = new Mock<ILogger<AutomatizadorCardsAnki.InsertCardsHandler>>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("DeckName", "deck1") })
                .Build();

            var conversation = new ConversationResponse
            {
                Text = "Meaning: a test meaning. Phrase: this is a test phrase."
            };

            openAiMock
                .Setup(x => x.CreateConversationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(conversation);

            openAiMock
                .Setup(x => x.GenerateImageUriAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("/tmp/image.png");

            ankiMock
                .Setup(x => x.RequestAnkiAsync(It.IsAny<AddNoteRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AnkiResponse { result = new JsonArray() });

            ankiMock
                .Setup(x => x.RequestAnkiAsync(It.IsAny<SyncAnkiWebRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AnkiResponse { result = new JsonArray() });

            var handler = new AutomatizadorCardsAnki.InsertCardsHandler(
                openAiMock.Object,
                ankiMock.Object,
                config,
                loggerMock.Object);

            var request = new InsertCardsRequest { Words = new List<string> { "test" } };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();

            ankiMock.Verify(x => x.RequestAnkiAsync(It.IsAny<AddNoteRequestDto>(), It.IsAny<CancellationToken>()), Times.Once);
            ankiMock.Verify(x => x.RequestAnkiAsync(It.IsAny<SyncAnkiWebRequestDto>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateConversationThrows_ReturnsFailure()
        {
            // Arrange
            var openAiMock = new Mock<IOpenAiApiManager>();
            var ankiMock = new Mock<IAnkiApiManager>();
            var loggerMock = new Mock<ILogger<AutomatizadorCardsAnki.InsertCardsHandler>>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("DeckName", "deck1") })
                .Build();

            openAiMock
                .Setup(x => x.CreateConversationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.InvalidOperationException("openai-failed"));

            var handler = new AutomatizadorCardsAnki.InsertCardsHandler(
                openAiMock.Object,
                ankiMock.Object,
                config,
                loggerMock.Object);

            var request = new InsertCardsRequest { Words = new List<string> { "test" } };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("openai-failed");

            ankiMock.Verify(x => x.RequestAnkiAsync(It.IsAny<AddNoteRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
            ankiMock.Verify(x => x.RequestAnkiAsync(It.IsAny<SyncAnkiWebRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
