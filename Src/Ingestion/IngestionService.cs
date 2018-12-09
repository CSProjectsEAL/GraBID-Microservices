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
    public class IngestionService
    {
        private IDictionary<string,IngestionSource> _sourcesList;
        private const string SourcesConfigRelativePath = "../../../../../config/IngestionSourcesTest.json";
        private ILogger _logger;
        private static IConnection _conn;
        private static IModel _channel;

        public IngestionService(IDictionary<string, IngestionSource> sources, ILogger logger)
        {
            _logger = logger;
            IConnectionFactory factory = new ConnectionFactory() {HostName = "localhost", UserName = "guest", Password = "guest"};
            _conn = factory.CreateConnection();
            _channel = _conn.CreateModel();
            _sourcesList = sources;
        }

        public IngestionService(ILogger logger)
        {
            _logger = logger;
            _sourcesList = LoadSourcesFromJson(SourcesConfigRelativePath);
            IConnectionFactory factory = new ConnectionFactory() {HostName = "localhost", UserName = "guest", Password = "guest"};
            _conn = factory.CreateConnection();
            _channel = _conn.CreateModel();
        }

        private async Task<string> GetData (string url)
        {
            var data = "";
            using (var client = new HttpClient())
            {
                using (var res = await client.GetAsync(url))
                {
                    using (var content = res.Content)
                    {
                        data = await content.ReadAsStringAsync();
                    }
                }
            }

            return data;
        }

        public IDictionary<string, IngestionSource> LoadSourcesFromJson(string path)
        {
            IDictionary<string, IngestionSource> sources = new Dictionary<string, IngestionSource>();
            // deserialize JSON directly from a file
            if (File.Exists(path))
            {
                string JSONText = File.ReadAllText(path);
                IList<IngestionSource> listsources = JsonConvert.DeserializeObject<IList<IngestionSource>>(JSONText);
                foreach (IngestionSource s in listsources) {    
                    sources.Add(s.Name, s);
                }
            }
            else
            {
                throw new Exception("Cant find file");
            }
            return sources;
        }

        public void Ingest(string sourceId) {

            if (!_sourcesList.ContainsKey(sourceId)) throw new Exception("Key was not found, source is not defined");
            
            _logger.Information("Starting Ingestor");

            var data = GetData(_sourcesList[sourceId].ApiUrl).Result;
            
            ForwardMessageToRabbitMQ(data, _sourcesList[sourceId].ForwardMessageQueue);
            
            _logger.Information("Stopping Ingestor");
        }

        public void ForwardMessageToRabbitMQ(string message, string queue)
        {
            using (_conn)
            {
                using (_channel)
                {
                    var exchange = "mono.data.received";
                    
                    _channel.QueueDeclare(queue, true, false, false, null);
                    
                    _channel.ExchangeDeclare(exchange, "fanout");
                    
                    _channel.QueueBind(queue, exchange, "");
                    
                    var envelope = new Envelope<string>(Guid.NewGuid(), message);
                    var envelopedMessage = JsonConvert.SerializeObject(envelope);
                    
                    byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(envelopedMessage);

                    _channel.BasicPublish(exchange, "", _channel.CreateBasicProperties(), messageBodyBytes);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", exchange, message);
                }
            }
        }

        public string SerializeObjects(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public void StoreObject(string obj) { 
            try {
                File.WriteAllText(SourcesConfigRelativePath, obj);
            } catch(Exception exception)
            {
                Console.WriteLine("Couldnt write to file: " + exception.Message + "Message");
            }
        }
    }
}
