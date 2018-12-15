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
            string sourceID = "";
            /*
            try {
                sourceID = args[0];
            } catch (Exception e) {
                logger.Error("Incorrect argument for ingestion dll crontab: " + e.Message);
            }
            */
            sourceID = "ResellerData";
            if (sourceID != ""){
                IngestionService ingestService = new IngestionService(logger);

                try {
                    while (true) { 
                        ingestService.Ingest(sourceID);
                        Thread.Sleep(3000);
                    }
                } catch(Exception e)
                {
                    logger.Error("Error: " + e.Message + "\n" + "Exception trace: " + e.StackTrace);
                }
            }
        }

    }
}
