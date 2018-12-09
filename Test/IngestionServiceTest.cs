using NUnit.Framework;
using Ingestion;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
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
                "Test",new IngestionSource() { ApiUrl = "Example", Credential = "blabla", ForwardMessageQueue = "monoqueue", Name = "Test"}
            );
            sources.Add(
                "Test2", new IngestionSource() { ApiUrl = "Example2", Credential = "blabla2", ForwardMessageQueue = "monoqueue2", Name = "Test2" }
                );
            sources.Add(
                "FakeData", new IngestionSource() { ApiUrl = "https://jsonplaceholder.typicode.com/posts", Credential = "", ForwardMessageQueue = "mono.data.received", Name = "FakeData" }
                );

            service = new IngestionService(sources, new LoggerConfiguration().WriteTo.Console().CreateLogger());
        }

        [Test]
        public void CanIngestFromEndpoint()
        {
            service.Ingest("FakeData");
            using (IConnection conn = new ConnectionFactory() {HostName = "localhost", UserName = "guest", Password = "guest"}.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    BasicGetResult result = channel.BasicGet("mono.data.received", false);
                    Assert.IsNotNull(result.Body);
                }
            }
        }

        [Test]
        public void TestCanLoadSourcesFromJSONFile()
        {
            IDictionary<string, IngestionSource> sources = service.LoadSourcesFromJson("../../../../config/IngestionSourcesTest.json");
            Assert.AreEqual(sources["ResellerData"].Name, "ResellerData");
            Assert.AreEqual(sources["ResellerData2"].Name, "ResellerData2");
            Assert.AreEqual(sources["FakeData"].Name, "FakeData");
        }
    }
}
