using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace teamsBot.Services
{
    public class BotService
    {
        private IConfiguration configuration;
        [BsonId]
        public string id { get; set; }
        public string name { get; set; }
        public string serviceUrl { get; set; }
        public string conversationId { get; set; }
        public ChannelAccount account { get; set; }
        public ConversationAccount conversation { get; set; } 

        public BotService(IConfiguration configuration)
        {
            this.configuration = configuration;

        }
        public void Join()
        {
            var appCredentials = new MicrosoftAppCredentials(configuration);
            var connector = new ConnectorClient(new Uri(serviceUrl), appCredentials);

        }

        public void Save()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("chatbot");
            var collection = database.GetCollection<BsonDocument>("bot");
            var botServiceDocument = new BsonDocument(new Dictionary<string,string>{{"serviceUrl",serviceUrl}});
            collection.InsertOne(botServiceDocument);
        }
    }
}