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
        _client.SlashCommandExecuted += OnSlashCommandExecuted;
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
        _ = RegisterCommandsAsync();
        return Task.CompletedTask;
    }
    private async Task RegisterCommandsAsync()
    {
        // AddChoice cria um dropdown fixo no Discord — o usuário não digita
        // livremente, só escolhe uma das opções que você define aqui.
        var tipoOption = new SlashCommandOptionBuilder()
            .WithName("tipo")
            .WithDescription("Tipo do canal (padrão: texto)")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(false)
            .AddChoice("Texto", "texto")
            .AddChoice("Voz", "voz");

        var criarCanalCommand = new SlashCommandBuilder()
            .WithName("criar-canal")
            .WithDescription("Cria um novo canal (texto ou voz) no servidor")
            .AddOption("nome", ApplicationCommandOptionType.String,
                "Nome do canal a ser criado", isRequired: true)
            .AddOption(tipoOption)
            .AddOption("categoria", ApplicationCommandOptionType.String,
                "Nome de uma categoria existente onde colocar o canal (opcional)", isRequired: false);

        // Registro por servidor (guild) fica disponível instantaneamente — ótimo pra testar.
        // Registro global (client.Rest.CreateGlobalApplicationCommand) demora até 1h pra propagar.
        foreach (var guild in _client.Guilds)
        {
            try
            {
                await guild.CreateApplicationCommandAsync(criarCanalCommand.Build());
                _logger.LogInformation("Comando /criar-canal registrado em {GuildName}", guild.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao registrar comando em {GuildName}", guild.Name);
            }
        }
    }

    private async Task OnSlashCommandExecuted(SocketSlashCommand command)
    {
        if (command.CommandName != "criar-canal")
            return;

        // SocketGuildUser dá acesso aos cargos/permissões do usuário NAQUELE servidor.
        // Se alguém rodar o comando numa DM (sem servidor), esse cast falha.
        if (command.User is not SocketGuildUser guildUser)
        {
            await command.RespondAsync("Esse comando só funciona dentro de um servidor.", ephemeral: true);
            return;
        }

        // Checagem de permissão: só quem já pode gerenciar canais no servidor pode usar o comando.
        if (!guildUser.GuildPermissions.ManageChannels)
        {
            await command.RespondAsync("⛔ Você não tem permissão para criar canais neste servidor.", ephemeral: true);
            return;
        }

        // DEPOIS
        var nome = (string)command.Data.Options.First(o => o.Name == "nome").Value;

        var tipoOption = command.Data.Options.FirstOrDefault(o => o.Name == "tipo");
        var tipo = tipoOption?.Value as string ?? "texto"; // "texto" é o padrão se o campo não for preenchido

        var categoriaOption = command.Data.Options.FirstOrDefault(o => o.Name == "categoria");
        var categoriaNome = categoriaOption?.Value as string;

        await command.DeferAsync(ephemeral: true);

        try
        {
            var guild = guildUser.Guild;

            SocketCategoryChannel? categoria = null;
            if (!string.IsNullOrWhiteSpace(categoriaNome))
            {
                categoria = guild.CategoryChannels
                    .FirstOrDefault(c => c.Name.Equals(categoriaNome, StringComparison.OrdinalIgnoreCase));
            }

            string mencaoCanal;

            if (tipo == "voz")
            {
                var novoCanalVoz = await guild.CreateVoiceChannelAsync(nome, props =>
                {
                    if (categoria != null)
                        props.CategoryId = categoria.Id;
                });
                mencaoCanal = $"<#{novoCanalVoz.Id}>";
            }
            else
            {
                var novoCanalTexto = await guild.CreateTextChannelAsync(nome, props =>
                {
                    if (categoria != null)
                        props.CategoryId = categoria.Id;
                });
                mencaoCanal = novoCanalTexto.Mention;
            }

            await command.FollowupAsync($"✅ Canal de {tipo} criado: {mencaoCanal}", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar canal");
            await command.FollowupAsync($"⚠️ Não consegui criar o canal: {ex.Message}", ephemeral: true);
        }
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