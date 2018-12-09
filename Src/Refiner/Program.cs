
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
        private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("name", typeof(Program).Assembly.GetName().Name)
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Starting...");
            
            RefinerService refiner = new RefinerService(Log.Logger);
            
            refiner.RetrieveMessageFromQueue("mono.data.received");
            
            Log.Information("Started");

            WaitHandle.WaitOne();
        }
    }
}
