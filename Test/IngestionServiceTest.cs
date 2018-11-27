using NUnit.Framework;
using Ingestion;
using System.Collections.Generic;
using Serilog;

namespace Tests
{
    public class IngestionServiceTest
    {
        private IngestionService service;

        [SetUp]
        public void Setup()
        {
            Dictionary<string, IngestionSource> sources = new Dictionary<string, IngestionSource>();
            sources.Add(
                "Test",new IngestionSource() { ApiUrl = "Example", Credential = "blabla", ForwardMessageQueue = "monoqueue", name = "Test"}
            );
            sources.Add(
                "Test2", new IngestionSource() { ApiUrl = "Example2", Credential = "blabla2", ForwardMessageQueue = "monoqueue2", name = "Test2" }
                );

            service = new IngestionService(sources, new LoggerConfiguration().WriteTo.Console().CreateLogger());
        }

        [Test]
        public void TestCanLoadSourcesFromJSONFile()
        {
            IDictionary<string, IngestionSource> sources = service.LoadSourcesFromJson("../../../../config/IngestionSourcesTest.json");
            Assert.AreEqual(sources["ResellerData"].name, "ResellerData");
            Assert.AreEqual(sources["ResellerData2"].name, "ResellerData2");
        }

        [Test]
        public void TestCanForwardMessageToQueue()
        {
            service.LoadSourcesFromJson("../../../../config/IngestionSourcesTest.json");
            service.ForwardMessageToRabbitMQ("");
        }

    }
}