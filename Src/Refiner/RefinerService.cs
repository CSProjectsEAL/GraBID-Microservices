using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using Shared;

namespace Refiner
{
    public class RefinerService
    {
        private ILogger _logger;
        private static IConnection _conn;
        private static IModel _channel;
        private const string Exchange = "mono.data.received ";

        public RefinerService(ILogger logger, IConnectionFactory factory)
        {
            _logger = logger;;
            _conn = factory.CreateConnection();
            _channel = _conn.CreateModel();
        }
        
        public void RetrieveMessageFromQueue (string queue)
        {
            using (_conn)
            {
                using (_channel)
                {
                    _channel.QueueDeclare(queue, true, false, false, null);

                    _channel.ExchangeDeclare(Exchange, "fanout");

                    _channel.QueueBind(queue, Exchange, "");

                    var consumer = new EventingBasicConsumer(_channel);

                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var key = ea.RoutingKey;
                        Console.WriteLine($" [x] Received '{key}':'{message}'");
                        var envelope = JsonConvert.DeserializeObject<Envelope<string>>(message);

                        var processor = new SubscriptionDataProcessor(envelope);
                        var cleanData = processor.CleanData();
                        
                        ForwardMessageToQueue("mono.data.refined", cleanData);
                        
                        Console.WriteLine($" [x] Sent 'mono.data.refined':'{cleanData}'");
                    };
                    
                    _channel.BasicConsume(queue, true, consumer);
                }
            }
        }

        public void ForwardMessageToQueue (string queue, JToken cleanData)
        {
            var message = JsonConvert.SerializeObject(cleanData);
            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(message);
            
            using (_conn)
            {
                using (_channel)
                {
                    _channel.QueueDeclare(queue, true, false, false, null);
                    
                    _channel.ExchangeDeclare(Exchange, "fanout");
                    
                    _channel.QueueBind(queue, Exchange, "");

                    _channel.BasicPublish(queue, "", _channel.CreateBasicProperties(), messageBodyBytes);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", Exchange, message);
                }
            }
            
        }
    }
}