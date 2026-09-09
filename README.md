# DiscordBotManager

Bot do Discord em C# (.NET) que responde dúvidas sobre um assunto específico usando IA (via API compatível com o padrão OpenAI, como Groq ou DeepSeek), e também gerencia a criação de canais do servidor por comando.

##  Funcionalidades

- **Tira-dúvidas por menção**: mencione o bot (`@NomeDoBot sua pergunta`) em qualquer canal e ele responde usando IA
- **Escopo restrito**: o bot é configurado para responder apenas sobre um assunto específico (definido em `BotPersona`), recusando educadamente perguntas fora do tema
- **Memória de conversa**: mantém o histórico recente de cada canal, para respostas com contexto
- **`/criar-canal`**: slash command para criar canais de **texto** ou **voz**, com opção de categoria, restrito a usuários com permissão de gerenciar canais no servidor

##  Tecnologias

- [.NET 8](https://dotnet.microsoft.com/download)
- [Discord.Net](https://github.com/discord-net/Discord.Net) — conexão com a API/Gateway do Discord
- API de IA compatível com o formato OpenAI Chat Completions (testado com [Groq](https://console.groq.com) e [DeepSeek](https://platform.deepseek.com))

##  Pré-requisitos

- .NET 8 SDK instalado
- Uma aplicação criada no [Discord Developer Portal](https://discord.com/developers/applications), com um Bot Token
- Uma API key de um provedor compatível com OpenAI (ex: [Groq](https://console.groq.com/keys), gratuito para começar)

##  Configuração

1. Clone o repositório:
   ```bash
   git clone https://github.com/natanrodriguesz/DiscordBotManager.git
   cd DiscordBotManager/DiscordBot
   ```

2. Copie o arquivo de exemplo de configuração e preencha com seus dados:
   ```bash
   cp appsettings.example.json appsettings.json
   ```

   ```json
   {
     "Discord": {
       "Token": "SEU_TOKEN_DO_BOT"
     },
     "DeepSeek": {
       "ApiKey": "SUA_API_KEY",
       "Model": "openai/gpt-oss-120b",
       "ApiUrl": "https://api.groq.com/openai/v1/chat/completions",
       "MaxHistoryMessages": 10
     },
     "BotPersona": {
       "Subject": "defina aqui o assunto que o bot deve dominar",
       "Tone": "amigável, direto e didático",
       "AllowSmallTalk": true,
       "OutOfScopeMessage": "Isso foge do assunto que eu conheço por aqui!"
     }
   }
   ```

   > `appsettings.json` está no `.gitignore` — suas credenciais nunca são versionadas.

3. No **Discord Developer Portal**, na aba **Bot**, habilite **MESSAGE CONTENT INTENT**.

4. Gere o link de convite em **OAuth2 → URL Generator**:
   - Escopos: `bot`, `applications.commands`
   - Permissões do bot: `Send Messages`, `Read Message History`, `View Channels`, `Manage Channels`
   - Abra a URL gerada e adicione o bot ao seu servidor

##  Rodando

```bash
dotnet restore
dotnet run
```

O console deve mostrar `Bot conectado como <nome>`. No Discord:

```
@SeuBot como funciona X?
```

```
/criar-canal nome:duvidas-gerais tipo:Texto
```

##  Estrutura do projeto

```
DiscordBot/
├── Program.cs                     → monta configuração e injeção de dependência
├── Config/
│   └── BotConfig.cs               → modelos de configuração (Discord / IA / Persona)
├── Models/
│   ├── ChatMessage.cs             → mensagem no formato role/content
│   └── ChatCompletionModels.cs    → request/response da API de IA
└── Services/
    ├── ConversationStore.cs       → histórico de conversa por canal (em memória)
    ├── SystemPromptBuilder.cs     → monta o prompt de escopo/persona restrita
    ├── DeepSeekService.cs         → chamada HTTP à API de IA (Groq/DeepSeek)
    └── DiscordBotService.cs       → conexão com o Discord, menções e slash commands
```

##  Permissões e segurança

- O bot funciona com permissões específicas (não requer Administrator) — veja a lista acima
- Quem executa `/criar-canal` precisa ter a permissão `Manage Channels` no servidor
- **Nunca** compartilhe seu Bot Token ou API key publicamente (em prints, mensagens, commits, etc). Se um token vazar, revogue-o imediatamente em Developer Portal → Bot → Reset Token

##  Roadmap / ideias futuras




