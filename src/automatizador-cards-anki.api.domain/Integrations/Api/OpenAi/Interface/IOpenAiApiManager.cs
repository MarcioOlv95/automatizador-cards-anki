namespace automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;

public interface IOpenAiApiManager
{
    Task<string> CreateConversationAsync(string question, CancellationToken cancellationToken);
}
