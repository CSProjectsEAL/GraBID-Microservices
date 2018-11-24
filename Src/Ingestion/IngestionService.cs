using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Serilog;
using Shared;

namespace Ingestion
{
    //Read about Rabbitmq - https://www.rabbitmq.com/tutorials/tutorial-five-dotnet.html
    // for more advanced Message Bus setup - http://masstransit-project.com/MassTransit/ which integrates with RabbitMQ as well
    public class IngestionService
    {
        private IDictionary<int,IngestionSource> _sourcesList;
        private const string _sourcesConfigRelativePath = "config/ingestion_sources.json";
        private ILogger _logger;
        private static string rabbitmqHostname = "rabbit.docker";

        public IngestionService(IDictionary<int, IngestionSource> sources)
        {

            _logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            _sourcesList = sources;
        }
        private async Task<string> GetDataAsync(string baseUrl)
        {
            string data = "";

            HttpClient client = new HttpClient();

            try
            {
                data = await client.GetStringAsync(baseUrl);

                Console.WriteLine(data);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            client.Dispose();
            return data;
        }

        public IngestionService()
        {
            _logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            _sourcesList = LoadSourcesFromJson();
        }

        // Due to sercurity concerns sources are either injected 
        //or loaded from config files
        private IDictionary<int, IngestionSource> LoadSourcesFromJson() 
        {
            IDictionary<int, IngestionSource> sources = null;
            // deserialize JSON directly from a file
            if (File.Exists(_sourcesConfigRelativePath)) {
                string JSONText = File.ReadAllText(_sourcesConfigRelativePath);
                sources = JsonConvert.DeserializeObject<Dictionary<int, IngestionSource>>(JSONText);
            }
            else {
                throw new Exception("Cant find file");
            } 
            return sources;
        }

        public void Ingest(int sourceID) {

            if (!_sourcesList.ContainsKey(sourceID)) throw new Exception("Key was not found, source is not defined");
            // Fetch data
            Task<string> data = GetDataAsync(_sourcesList[sourceID].ApiUrl);
            string dataString = data.Result.ToString();

            _logger.Information("Starting Ingestor");
            ForwardMessageToRabbitMQ(dataString);
            _logger.Information("Stopping Ingestor");
        }

        private void ForwardMessageToRabbitMQ(string message)
        {
            var factory = new ConnectionFactory() { HostName = rabbitmqHostname };
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    var exchange = "grabid_exchange";
                    var routingKey = "mono.data.received";

                    channel.ExchangeDeclare(exchange: exchange, type: "topic");
                    var envelope = new Envelope<string>(Guid.NewGuid(), message);
                    var envelopedMessage = JsonConvert.SerializeObject(envelope);
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(envelopedMessage);

                    channel.BasicPublish(exchange, routingKey, null, messageBodyBytes);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
                }
            }
        }

        public string SerializeObjects(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public void StoreObject(string obj) { 
            try {
                File.WriteAllText(_sourcesConfigRelativePath, obj);
            } catch(Exception exception)
            {
                Console.WriteLine("Couldnt write to file: " + exception.Message + "Message");
            }
        }
    }
}
