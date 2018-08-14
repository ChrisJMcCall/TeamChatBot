using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;
using teamsBot.Models;
using teamsBot.Services;

namespace teamsBot.Controllers
{
    
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private IConfiguration configuration;
        private readonly BotService botService;
        private static ConversationAccount conversation;

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
                    // This is the only point the bot connection info gets saved so that it can
                    // be retrieved later. I would like to have another reliable place to get the 
                    // information without reliance on the join event, but this is all I can find so far.

                    this.botService.serviceUrl = activity.ServiceUrl;
                    this.botService.conversationId = activity.Conversation.Id;
                    this.botService.account = activity.Recipient;
                    this.botService.Save();
                    
                    var appCredentials = new MicrosoftAppCredentials(configuration);
                    var connector = new ConnectorClient(new Uri(activity.ServiceUrl), appCredentials);
                    Activity newActivity = activity.CreateReply("Chime in");

                    await connector.Conversations.SendToConversationAsync(activity.Conversation.Id, newActivity);
                }
            }
            else
            {
                //HandleSystemMessage(activity);
            }
            return Ok();
        }

        [HttpPost("webhook")]
        public async Task<OkResult> Webhook([FromBody] WebHookEvent jsonData)
        {
            this.botService.Restore();
            var appCredentials = new MicrosoftAppCredentials(configuration);
            MicrosoftAppCredentials.TrustServiceUrl(this.botService.serviceUrl);
            var connector = new ConnectorClient(new Uri(this.botService.serviceUrl), appCredentials);
            
            IMessageActivity newActivity = Activity.CreateMessageActivity();
            newActivity.From = this.botService.account;
            newActivity.ChannelId = "emulator";
            newActivity.Conversation = new ConversationAccount(id: this.botService.conversationId);
            newActivity.Text = jsonData.eventType;

            await connector.Conversations.SendToConversationAsync(this.botService.conversationId, (Activity)newActivity);

            return Ok();
        }


    }

}