using Microsoft.Bot.Connector;

namespace teamsBot.Models
{
    public class Bot
    {
        public string id { get; set; }
        public string name { get; set; }
        public string serviceUrl { get; set; }
        public string conversationId { get; set; }
        public ChannelAccount account { get; set; }
        public ConversationAccount conversation { get; set; }

    }
}