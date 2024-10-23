namespace automatizador_cards_anki.api.domain.Integrations.Api.Anki;

public class SyncAnkiWebRequestDto(string action, int version)
{
    public string action { get; } = action;
    public int version { get; } = version;
}
