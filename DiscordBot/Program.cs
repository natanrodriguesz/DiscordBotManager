using ConfigDiscord.Config;
using DiscordAIBot.Config;
using DiscordAIBot.Services;
using DiscordBot.Config;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using System.Net.Http;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var discordConfig = builder.Configuration.GetSection("Discord").Get<ConfigDiscord.Config.DiscordConfig>()
    ?? throw new InvalidOperationException("Seção 'Discord' não encontrada.");

var deepSeekConfig = builder.Configuration.GetSection("DeepSeek").Get<DiscordAIBot.Config.DeepSeekConfig>()
    ?? throw new InvalidOperationException("Seção 'DeepSeek' não encontrada.");


var personaConfig = builder.Configuration.GetSection("BotPersona").Get<BotPersonaConfig>()
    ?? throw new InvalidOperationException("Seção 'BotPersona' não encontrada.");

builder.Services.AddSingleton(discordConfig);
builder.Services.AddSingleton(deepSeekConfig);
builder.Services.AddSingleton(personaConfig);
builder.Services.AddSingleton<SystemPromptBuilder>();

builder.Services.AddHttpClient<DeepSeekService>();
builder.Services.AddSingleton(new ConversationStore(deepSeekConfig.MaxHistoryMessages));
builder.Services.AddHostedService<DiscordBotService>();

var host = builder.Build();
await host.RunAsync();