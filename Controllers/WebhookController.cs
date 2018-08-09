using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;

namespace teamsBot.Controllers
{
    [Route("api/[controller]")]
    public class WebhookController : Controller
    {
        private IConfiguration configuration;


        public WebhookController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            //MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
            var appCredentials = new MicrosoftAppCredentials(configuration);
            var connector = new ConnectorClient(appCredentials);

            Activity reply = new Activity();
            reply.Text = "test reply";

            await connector.Conversations.SendToConversationAsync(reply);

            return Ok();
        }

    }

}