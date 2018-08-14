using Newtonsoft.Json;

namespace teamsBot.Models
{
    public class WebHookEvent
    {
        public long timestamp { get; set; }
        public string webhookEvent { get; set; }
        [JsonProperty("issue_event_type_name")]
        public string eventType { get; set; }
        public object user { get; set; }
        public object issue { get; set; }
    }
}