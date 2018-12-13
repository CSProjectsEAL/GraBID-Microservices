using Newtonsoft.Json.Linq;

namespace Refiner
{
    public interface IDBFacade
    {
        void SaveCollection(JArray collection, string collectionName, string databaseName);
        string FetchCollection(string collectionName, string databaseName);
        string GetLatestID(string id, string collectionName, string databaseName);
    }
}
