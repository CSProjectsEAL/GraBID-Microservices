using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        private static bool _enableValidaiton;
        private ILogger _logger;
        private static IConnection _conn;
        private static IModel _channel;
        private static string _exchange;
        private IList<DataSource> _dataSources;

        public RefinerService(ILogger logger, IConnectionFactory factory, string exchangeName, bool enableValidation = true)
        {
            _exchange = exchangeName;
            _enableValidaiton = enableValidation;
            _logger = logger;;
            _conn = factory.CreateConnection();
            _channel = _conn.CreateModel();
        }

        public void Start()
        {
            Log.Information("Adding listeners on RabbitMQ channels");
            foreach (DataSource s in _dataSources)
            {

            }
            Log.Information("Listeners successfully started.");
        }

        public async Task<bool> StartListening(DataSource datasource)
        {
            using (_conn)
            {
                using (_channel)
                {
                    _channel.QueueDeclare(queue, true, false, false, null);

                    _channel.ExchangeDeclare(_exchange, "fanout");

                    _channel.QueueBind(queue, _exchange, "");

                    var consumer = new EventingBasicConsumer(_channel);

                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var key = ea.RoutingKey;
                        Console.WriteLine($" [x] Received '{key}':'{message}'");
                        var envelope = JsonConvert.DeserializeObject<Envelope<string>>(message);

                        datasource.Process();
                        var datahandlers = new ExtractSubscribersProcessor(envelope);
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
                    
                    _channel.ExchangeDeclare(_exchange, "fanout");
                    
                    _channel.QueueBind(queue, _exchange, "");

                    _channel.BasicPublish(queue, "", _channel.CreateBasicProperties(), messageBodyBytes);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", _exchange, message);
                }
            }
            
        }
    }
}