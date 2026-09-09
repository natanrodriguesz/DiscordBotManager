namespace DiscordAIBot.Config;

// Guarda as configurações de conexão com a API da Groq
public class DeepSeekConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "openai/gpt-oss-120b";
    public string ApiUrl { get; set; } = "https://api.groq.com/openai/v1/chat/completions";
    public int MaxHistoryMessages { get; set; } = 10;
}
// Guarda a persona/escopo do bot (o que ele sabe responder)
public class BotPersonaConfig
{
    public string Subject { get; set; } = "administraçao , dicas, gestão de servidores do discord";
    public string Tone { get; set; } = "amigável, direto e didático ";
    public bool AllowSmallTalk { get; set; } = true;
    public string OutOfScopeMessage { get; set; } =
        "Isso foge do assunto que eu conheço aqui.";
}