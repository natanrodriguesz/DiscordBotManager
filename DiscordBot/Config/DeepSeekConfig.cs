using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Config
{
    public class DeepSeekConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "deepseek-chat";
        public string ApiUrl { get; set; } = "https://api.deepseek.com/chat/completions";
        public int MaxHistoryMessages { get; set; } = 10;
    }
}
