using automatizador_cards_anki.api.domain.Helper;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi.Interface;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Images;

namespace automatizador_cards_anki.api.integrations.openai;

public class OpenAiApiManager(IConfiguration configurattion) : IOpenAiApiManager
{
    private readonly OpenAIClient _client = new(configurattion.GetSection("Chaves:OpenAi").Value!);
    private readonly ImageClient _imageClient = new("gpt-image-1", configurattion.GetSection("Chaves:OpenAi").Value!);

    public async Task<ConversationResponse> CreateConversationAsync(string question, CancellationToken ct)
    {
        var response = await _client.GetChatClient("gpt-5.5").CompleteChatAsync(question);

        return new ConversationResponse { Text = response.Value?.Content[0]?.Text! };
    }

    public async Task<string> GenerateImageUriAsync(string question, string word, CancellationToken ct)
    {
        var result = await _imageClient.GenerateImageAsync(question, cancellationToken: ct);

        return ImageHelper.SaveToPathAndGetUri(result.Value.ImageBytes.ToArray(), word);
    }
}
