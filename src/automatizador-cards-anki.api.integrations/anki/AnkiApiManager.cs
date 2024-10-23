using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace automatizador_cards_anki.api.integrations.anki;

public class AnkiApiManager : IAnkiApiManager
{
    private readonly IConfiguration _configuration;
    private readonly string ENDPOINT;

    public AnkiApiManager(IConfiguration configuration)
    {
        _configuration = configuration;
        ENDPOINT = _configuration.GetValue<string>("Apis:Anki");
    }

    public async Task<AnkiResponse> RequestAnkiAsync<T>(T request, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();

        var json = JsonConvert.SerializeObject(request);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(ENDPOINT, content, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<AnkiResponse>(cancellationToken);

        return result!;
    }
}
