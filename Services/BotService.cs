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

        public BotService(IConfiguration configuration)
        {
            this.configuration = configuration;

        }
        public void Join()
        {
            var appCredentials = new MicrosoftAppCredentials(configuration);
            var connector = new ConnectorClient(new Uri(serviceUrl), appCredentials);
        }

        public void Restore()
        {
            string connectionString = configuration.GetConnectionString("mongodb");
            var dbName = new MongoUrl(connectionString).DatabaseName;
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>("bot");
            var filter = new BsonDocument();
            var botDocument = collection.Find(filter).Single();
            // id = botDocument.GetElement("id").ToString();
            serviceUrl = botDocument.GetElement("serviceUrl").Value.ToString();
            conversationId = botDocument.GetElement("conversationId").Value.ToString();
            account = new ChannelAccount(id: botDocument.GetElement("account").Value.ToString(), name: "Bot");
        }

        public void Save()
        {
            string connectionString = configuration.GetConnectionString("mongodb");
            var dbName = new MongoUrl(connectionString).DatabaseName;
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>("bot");
            var filter = new BsonDocument();
            var update = new BsonDocument("$set", new BsonDocument(new Dictionary<string,object>{
                {"serviceUrl",serviceUrl},
                {"conversationId", conversationId},
                {"account", account.Id}
                }));
            var options = new UpdateOptions { IsUpsert = true };
            var result = collection.UpdateOne(filter, update, options);
        }
    }
}