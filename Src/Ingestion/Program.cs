using Newtonsoft.Json;
using RabbitMQ.Client;
using Serilog;
using Shared;
using System;
using System.Threading;

namespace Ingestion
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            IngestionService ingestService = new IngestionService(logger);

            try
            {
                while (true)
                {
                    ingestService.Ingest("");
                    Thread.Sleep(3000);
                }
            }
            catch (Exception e)
            {
                logger.Error("Error: " + e.Message + "\n" + "Exception trace: " + e.StackTrace);
            }
        }

    }
}
