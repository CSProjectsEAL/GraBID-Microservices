using NUnit.Framework;
using Refiner;

namespace Tests
{
    public class RefinerServiceTest
    {
        [SetUp]
        public void Setup()
        {
            IDBFacade db = new MongoDBConnector("localhost");
        }

        [Test]
        public void CanProcessData()
        {
            Processor processor1 = new Processor();
            Processor processor2 = new Processor();
            Assert.Pass();
        }
    }
}