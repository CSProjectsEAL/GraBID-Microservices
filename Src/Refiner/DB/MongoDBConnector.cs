using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace Refiner
{
    public class MongoDBConnector : IDBFacade
    {
        private static string _connectionString;
        private static ILogger _logger;
        private static MongoClient _dbConnection;
        private MongoClient _MongoDBClient
        {
            get
            {
                if (_dbConnection == null)
                {
                    InitializeMongoDBConnection();
                }
                return _dbConnection;
            }
            set {
                _dbConnection = value;
            }
        }

        public MongoDBConnector(string connectionString)
        {
            _connectionString = connectionString;
        }

        private void InitializeMongoDBConnection()
        {
            try
            {
                if (_connectionString == "")
                {
                    throw new MongoClientException("Connection string was empty. /n");
                }
                _MongoDBClient = new MongoClient(_connectionString);
            }
            catch (MongoClientException e)
            {
                LogAndThrow("Connection refused. /n", e);
            }
        }

        private BsonDocument ConvertFromJsonToBson(string json)
        {
            return MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
        }

        public void SaveCollection(string collection, string collectionName, string databaseName)
        {
            try
            {
             var database = _MongoDBClient.GetDatabase(databaseName);
             IList<BsonDocument> collectionAsDocuments = ConvertJsonCollectionToBsonDocuments();
             var dbCollection = database.GetCollection<BsonDocument>(collectionName);
             dbCollection.InsertManyAsync(collectionAsDocuments);

            }
            catch (MongoClientException e)
            {
                LogAndThrow("Error: Couldn't save collection, " + collectionName + "/n", e);
            }
        }

        public string FetchCollection(string collectionName, string databaseName, string fromId = null)
        {
            string collectionToString = "";
            
            try
            {
                //Creates database if it doesn't exist
                var database = _MongoDBClient.GetDatabase(databaseName);
                collectionToString = database.GetCollection<BsonDocument>(collectionName).ToJson();
                
            }
            catch (MongoClientException e)
            {
                LogAndThrow("Error: Couldn't fetch collection, " + collectionName + "/n", e);
            }
            return collectionToString;
        }

        public string LatestID(string collectionName, string databaseName)
        {
            string id = null;
            try
            {
                //Creates database if it doesn't exist
                var database = _MongoDBClient.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);
                var document = collection.Find(new BsonDocument()).Sort("_id").FirstOrDefaultAsync();
                id = document.ToString();
            }
            catch (MongoClientException e)
            {
                LogAndThrow("Error: Couldn't get ID from collection, " + collectionName + "/n", e);
            }
            return id;
        }
        private void LogAndThrow(string msg, MongoException e)
        {
            string completeMsg = msg + ", Message: " + e.Message + ", Trace: " + "/n" + e.StackTrace;
            _logger.Error(completeMsg);
            throw new Exception(completeMsg);
        }

        public string FetchCollection(string collectionName, string databaseName)
        {
            throw new NotImplementedException();
        }
    }
}
