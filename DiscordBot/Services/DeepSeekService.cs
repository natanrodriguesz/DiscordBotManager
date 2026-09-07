using BotDiscord.Models;
using DiscordAIBot.Models;
using DiscordAIBot.Config;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DiscordAIBot.Services;

// Responsável por conversar com a API do deepseek, enviando o histórico de mensagens e recebendo a resposta do modelo. 
public class DeepSeekService
{
    private readonly HttpClient _http;
    private readonly DeepSeekConfig _config;
    private readonly string _systemPrompt;

    public DeepSeekService(HttpClient http, DeepSeekConfig config, SystemPromptBuilder promptBuilder)
    {
        _http = http;
        _config = config;
        _systemPrompt = promptBuilder.Build();

        _http.BaseAddress = new Uri(_config.ApiUrl);
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _config.ApiKey);
    }

    public async Task<string> AskAsync(List<ChatMessage> history, string userMessage)
    {
        var messages = new List<ChatMessage> { new("system", _systemPrompt) };
        messages.AddRange(history);
        messages.Add(new ChatMessage("user", userMessage));

        var request = new DeepSeekRequest { Model = _config.Model, Messages = messages };

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync(string.Empty, request);
        }
        catch (HttpRequestException ex)
        {
            return $"⚠️ Não consegui falar com a DeepSeek agora ({ex.Message}).";
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            return $"⚠️ Erro do DeepSeek ({(int)response.StatusCode}): {errorBody}";
        }

        var result = await response.Content.ReadFromJsonAsync<DeepSeekResponse>();
        var reply = result?.Choices.FirstOrDefault()?.Message.Content;

        return string.IsNullOrWhiteSpace(reply)
            ? " Resposta vazia, tenta reformular a pergunta."
            : reply.Trim();
    }
}