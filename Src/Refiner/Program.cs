
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

namespace Refiner
{
    class Program
    {
        //TODO: Refactor to environment variable config file
        private static string ConnectionString;
    

        private static void Instanciate() {
            ConnectionString = "";

            //Queue, processors
            IDictionary<string, List<IDataProcessor>> processors = new Dictionary<string, List<IDataProcessor>>();

            //Refiner service with definitions
            MongoDBConnector connector = new MongoDBConnector(ConnectionString);
            RService = new RefinerService(connector);
            
            // Logger
            Log.Logger = new LoggerConfiguration()
            .Enrich.WithProperty("name", typeof(Program).Assembly.GetName().Name)
            .WriteTo.Console()
            .CreateLogger();
        }

        private static void Startup()
        {
            Log.Information("Starting...");
            AssemblyLoadContext.Default.Unloading += _ => Exit();
            Console.CancelKeyPress += (_, __) => Exit();
            Instanciate();
        }

        static void Main(string[] args)
        {
            //Application Startup
            Startup();
        }

        private static void Exit()
        {
            Log.Information("Exiting...");
            channel.Close();
            conn.Close();
        }
    }

}
