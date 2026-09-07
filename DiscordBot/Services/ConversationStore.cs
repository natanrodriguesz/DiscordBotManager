using Microsoft.Graph.Models;
using System.Collections.Concurrent;

namespace DiscordAIBot.Services;

// Guarda em memória o histórico de mensagens de cada canal do Discord
public class ConversationStore
{
    private readonly int _maxMessages;
    private readonly ConcurrentDictionary<ulong, List<ChatMessage>> _history = new();

    public ConversationStore(int maxMessages)
    {
        _maxMessages = maxMessages;
    }

    public List<ChatMessage> GetHistory(ulong channelId)
    {
        return _history.GetOrAdd(channelId, _ => new List<ChatMessage>());
    }

    public void AddExchange(ulong channelId, string userMessage, string assistantReply)
    {
        var list = _history.GetOrAdd(channelId, _ => new List<ChatMessage>());

        lock (list)
        {
            list.Add(new ChatMessage("user", userMessage));
            list.Add(new ChatMessage("assistant", assistantReply));

            while (list.Count > _maxMessages)
                list.RemoveAt(0);
        }
    }
}