﻿using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using OpenAI_API.Models;

namespace automatizador_cards_anki.api.integrations.azure_openai;

public class OpenAiApiManager : IOpenAiApiManager
{
    private readonly OpenAIAPI _openAiApi;
    private readonly IConfiguration _configuration;
    private readonly string CHAVE_ACESSO;

    public OpenAiApiManager(IConfiguration configurattion)
    {
        _configuration = configurattion;
        CHAVE_ACESSO = _configuration.GetValue<string>("Chaves:OpenAi");
        _openAiApi = new OpenAIAPI(CHAVE_ACESSO);
    }

    public async Task<string> CreateConversationAsync(string question, CancellationToken cancellationToken)
    {
        try
        {
            var chat = _openAiApi.Chat.CreateConversation();
            chat.Model = Model.ChatGPTTurbo;
            chat.RequestParameters.Temperature = 1;
            chat.AppendUserInput(question);
            var response = await chat.GetResponseFromChatbotAsync();

            return response;
        }
        catch
        {
            throw;
        }
    }
}