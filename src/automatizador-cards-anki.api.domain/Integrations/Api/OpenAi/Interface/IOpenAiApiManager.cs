namespace automatizador_cards_anki.api.domain.Integrations.Api.OpenAi.Interface;

public interface IOpenAiApiManager
{
    Task<ConversationResponse> CreateConversationAsync(string question, CancellationToken cancellationToken);
    Task<string> GenerateImageUriAsync(string question, string word, CancellationToken cancellationToken);
}
