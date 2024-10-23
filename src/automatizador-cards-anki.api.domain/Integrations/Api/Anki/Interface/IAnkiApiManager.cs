namespace automatizador_cards_anki.api.domain.Integrations.Api.Anki;

public interface IAnkiApiManager
{
    Task<AnkiResponse> RequestAnkiAsync<T>(T request, CancellationToken cancellationToken);
}
