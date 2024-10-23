using System.Text.Json.Nodes;

namespace automatizador_cards_anki.api.domain.Integrations.Api.Anki;

public class AnkiResponse
{
    public JsonArray result { get; set; }
    public string? error { get; set; }
}
