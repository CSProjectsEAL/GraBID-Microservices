
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Refiner
{
    class Program
    {
        private static IConnection conn;

        private static IModel channel;
        private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            AssemblyLoadContext.Default.Unloading += _ => Exit();
            Console.CancelKeyPress += (_, __) => Exit();

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("name", typeof(Program).Assembly.GetName().Name)
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Starting...");
            
            RetrieveMessageFromQueue("mono.data.received");
            
            Log.Information("Started");

            WaitHandle.WaitOne();
        }

        public static void RetrieveMessageFromQueue (string queue)
        {
            var factory = new ConnectionFactory();
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    var exchange = "mono.data.received";

                    channel.QueueDeclare(queue, true, false, false, null);

                    channel.ExchangeDeclare(exchange: exchange, type: "fanout");

                    channel.QueueBind(queue, exchange, "");

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var key = ea.RoutingKey;
                        Console.WriteLine($" [x] Received '{key}':'{message}'");
                        var envelope = JsonConvert.DeserializeObject<Envelope<string>>(message);

                        var processor = new DataProcessor(envelope);
                        var cleanData = processor.ParseEnvelope();
                        
                        ForwardMessageToQueue(channel, "mono.data.refined", cleanData);
                        
                        Console.WriteLine($" [x] Sent 'mono.data.refined':'{cleanData}'");
                    };
                    
                    channel.BasicConsume(queue: queue,
                        autoAck: true,
                        consumer: consumer);
                }
            }
        }

        public static void ForwardMessageToQueue (IModel channel, string queue, JArray cleanData)
        {
            var message = JsonConvert.SerializeObject(cleanData);
            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(queue, null, null, messageBodyBytes);
        }
        
        private static void Exit()
        {
            Log.Information("Exiting...");
            channel.Close();
            conn.Close();
        }
    }
}
