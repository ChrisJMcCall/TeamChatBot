using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;
using teamsBot.Services;

namespace teamsBot.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private IConfiguration configuration;
        private readonly BotService botService;
        private static string channelId;
        private static string conversationId;
        private static string serviceUri;
        private static ConversationAccount conversation;
        private static ChannelAccount bot;

        public MessagesController(IConfiguration configuration, BotService botService)
        {
            this.configuration = configuration;
            this.botService = botService;
        }


        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                //MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
                var appCredentials = new MicrosoftAppCredentials(configuration);
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);

                // return our reply to the user
                var reply = activity.CreateReply("message reply");
                await connector.Conversations.ReplyToActivityAsync(reply);
            } 
            else if (activity.Type == ActivityTypes.Event)
            {
                var appCredentials = new MicrosoftAppCredentials(configuration);
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);

                Activity reply = activity.CreateReply("event reply");

                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (activity.MembersAdded.Count > 0 && activity.MembersAdded[0].Name == "Bot") {
                    this.botService.serviceUrl = activity.ServiceUrl;
                    this.botService.Save();
                    bot = activity.Recipient;
                    serviceUri = activity.ServiceUrl;
                    channelId = activity.ChannelId;
                    conversationId = activity.Conversation.Id;
                    
                    var appCredentials = new MicrosoftAppCredentials(configuration);
                    var connector = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);
                    Activity newActivity = activity.CreateReply("Chime in");

                    await connector.Conversations.SendToConversationAsync(conversationId, newActivity);
                }
            }
            else
            {
                //HandleSystemMessage(activity);
            }
            return Ok();
        }

        [HttpPost("webhook")]
        public async Task<OkResult> Webhook([FromBody] object jsonData)
        {
            this.botService.Join();

            var appCredentials = new MicrosoftAppCredentials(configuration);
            var connector = new ConnectorClient(new Uri(serviceUri), appCredentials);
            IMessageActivity newActivity = Activity.CreateMessageActivity();
            newActivity.From = bot;
            newActivity.Conversation = new ConversationAccount(id: conversationId);
            newActivity.Text = "Webhooks!";

            await connector.Conversations.SendToConversationAsync(conversationId, (Activity)newActivity);

            return Ok();
        }


    }

}