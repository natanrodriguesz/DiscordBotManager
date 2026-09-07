using BotDiscord.Models;
using System.Text.Json.Serialization;

namespace DiscordAIBot.Models;

// O corpo do JSON que enviamos na requisição
public class DeepSeekRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 500;
}

// O que a DeepSeek devolve de volta
public class DeepSeekResponse
{
    [JsonPropertyName("choices")]
    public List<DeepSeekChoice> Choices { get; set; } = new();
}

public class DeepSeekChoice
{
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; } = new();
}