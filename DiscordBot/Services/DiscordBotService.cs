using ConfigDiscord.Config;
using Discord;
using Discord.WebSocket;
using DiscordAIBot.Services;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace DiscordBot.Services;

// Gerencia a conexão com o Discord e reage quando o bot é mencionado
public class DiscordBotService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly ConfigDiscord.Config.DiscordConfig _discordConfig;
    private readonly DeepSeekService _deepSeekService;
    private readonly ConversationStore _conversationStore;
    private readonly ILogger<DiscordBotService> _logger;

    public DiscordBotService(
          ConfigDiscord.Config.DiscordConfig discordConfig,
    DeepSeekService deepSeekService,
    ConversationStore conversationStore,
    ILogger<DiscordBotService> logger)
    {
        _discordConfig = discordConfig;
        _deepSeekService = deepSeekService;
        _conversationStore = conversationStore;
        _logger = logger;

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
                            | GatewayIntents.GuildMessages
                            | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);
        _client.Log += OnLog;
        _client.Ready += OnReady;
        _client.MessageReceived += OnMessageReceived;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _client.LoginAsync(TokenType.Bot, _discordConfig.Token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private Task OnLog(LogMessage log)
    {
        _logger.LogInformation("{Source}: {Message}", log.Source, log.Message);
        return Task.CompletedTask;
    }

    private Task OnReady()
    {
        _logger.LogInformation("Bot conectado como {Username}", _client.CurrentUser.Username);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot)
            return;

        bool wasMentioned = message.MentionedUsers.Any(u => u.Id == _client.CurrentUser.Id);
        if (!wasMentioned)
            return;

        string cleanText = message.Content
            .Replace(_client.CurrentUser.Mention, string.Empty)
            .Trim();

        if (string.IsNullOrWhiteSpace(cleanText))
        {
            await message.Channel.SendMessageAsync("Fala! Pode perguntar alguma coisa ");
            return;
        }

        using (message.Channel.EnterTypingState())
        {
            var history = _conversationStore.GetHistory(message.Channel.Id);
            var reply = await _deepSeekService.AskAsync(history, cleanText);
            _conversationStore.AddExchange(message.Channel.Id, cleanText, reply);
            await message.Channel.SendMessageAsync(reply);
        }
    }
}