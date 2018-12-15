using NUnit.Framework;
using Refiner;

namespace Tests
{
    public class MongoDBConnectorTests
    {
        [Test]
        public void CanSaveAndRetrieveJSONCollection()
        {
            IDBFacade db = new MongoDBConnector("localhost");
            string jsonCollection = "{\"employees\":[{\"firstName\":\"John\", \"lastName\":\"Doe\"},{\"firstName\":\"Anna\", \"lastName\":\"Smith\"},{\"firstName\":\"Peter\",\"lastName\":\"Jones\"}]}";
            db.SaveCollection(jsonCollection, "Test", "RefinedCache");
            string fetchedData  = db.FetchCollection("Test", "RefinedCache");
            Assert.AreEqual(jsonCollection, fetchedData);
        }
    }
}