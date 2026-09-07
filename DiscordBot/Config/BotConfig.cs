namespace DiscordAIBot.Config;

// Guarda as configurações de conexão com a API da Groq
public class DeepSeekConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "deepseek-chat";
    public string ApiUrl { get; set; } = "https://api.deepseek.com/chat/completions";
    public int MaxHistoryMessages { get; set; } = 10;
}
// Guarda a persona/escopo do bot (o que ele sabe responder)
public class BotPersonaConfig
{
    public string Subject { get; set; } = "administraçao e gestão de servidores do discord";
    public string Tone { get; set; } = "amigável, direto e didático ";
    public bool AllowSmallTalk { get; set; } = true;
    public string OutOfScopeMessage { get; set; } =
        "Isso foge do assunto que eu conheço aqui.";
}