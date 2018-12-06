using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Refiner
{
    public class RefinerService
    {
        private static string exchange = "grabid_exchange";
        private static string routingKey = "mono.data.received";
        private static IDBFacade _db;
        // Definition of sources and their processors
        private static IDictionary<string, IList<IDataProcessor>> _dataSources;

        private static IConnection conn;

        private static IModel channel;

        private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

        public RefinerService(IDBFacade db)
        {
            _dataSources = dataSources;
            _db = db;
        }
        // Listen to queues
        public static void StartListening()
        {
            channel.ExchangeDeclare(exchange: exchange, type: "topic");


            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName,
                                  exchange: exchange,
                                  routingKey: routingKey);

            var consumer = new EventingBasicConsumer(channel);
            
        }


        // Establish connection
        public void ConnectToRabbitMQ() {
            var factory = new ConnectionFactory() { HostName = "rabbit.docker" };
            conn = factory.CreateConnection();
            channel = conn.CreateModel();
        }

        // Process data
        public void ProcessData() {

            consumer.Received += (model, ea) =>
            {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);
            var key = ea.RoutingKey;
            Log.Information($" [x] Received '{key}':'{message}'");
            var envelope = JsonConvert.DeserializeObject<Envelope<string>>(message);

            dynamic json = JObject.Parse(envelope.Payload);
            string messageString = json.message;
            string userString = json.users;
            string[] userArray = userString.Split(",");
            string[] messages = userArray.Select(user => { return $"{messageString} {user}"; }).ToArray();
            var returnEnvelope = new Envelope<string[]>(envelope.Id, messages);
            string newMessage = JsonConvert.SerializeObject(returnEnvelope);
            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(newMessage);



            channel.BasicPublish(exchange, "mono.data.refined", null, messageBodyBytes);
            Log.Information($" [x] Sent 'mono.data.refined':'{newMessage}'");

            };

        }

        // Store data, when a new batch of data is received
        public void StoreData() {

        
        }
    }
}
