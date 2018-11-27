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
        private IDictionary<string,IngestionSource> _sourcesList;
        private const string _sourcesConfigRelativePath = "../../../../../config/IngestionSourcesTest.json";
        private ILogger _logger;
        private static string _rabbitmqHostname = "rabbit.docker";

        public IngestionService(IDictionary<string, IngestionSource> sources, ILogger logger)
        {
            _logger = logger;
            _sourcesList = sources;
        }

        public IngestionService(ILogger logger)
        {
            _logger = logger;
            _sourcesList = LoadSourcesFromJson(_sourcesConfigRelativePath);
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

        // Due to sercurity concerns sources are either injected 
        //or loaded from config files

        public IDictionary<string, IngestionSource> LoadSourcesFromJson(string path)
        {
            IDictionary<string, IngestionSource> sources = new Dictionary<string, IngestionSource>();
            // deserialize JSON directly from a file
            if (File.Exists(path))
            {
                string JSONText = File.ReadAllText(path);
                IList<IngestionSource> listsources = JsonConvert.DeserializeObject<IList<IngestionSource>>(JSONText);
                foreach (IngestionSource s in listsources) {    
                    sources.Add(s.name, s);
                }
            }
            else
            {
                throw new Exception("Cant find file");
            }
            return sources;
        }

        public void Ingest(string sourceID) {

            if (!_sourcesList.ContainsKey(sourceID)) throw new Exception("Key was not found, source is not defined");
            // Fetch data
            //Task<string> data = GetDataAsync(_sourcesList[sourceID].ApiUrl);
            //string dataString = data.Result.ToString();
            string dataString = "message test";
            _logger.Information("Starting Ingestor");
            ForwardMessageToRabbitMQ(dataString);
            _logger.Information("Stopping Ingestor");
        }

        public void ForwardMessageToRabbitMQ(string message)
        {
            var factory = new ConnectionFactory() { HostName = _rabbitmqHostname };
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
