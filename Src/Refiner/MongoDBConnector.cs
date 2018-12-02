using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver.Core;

namespace Refiner
{
    public class MongoDBConnector : IDBFacade
    {

        private string _connectionString;
        public MongoDBConnector(string connectionString)
        {
            _connectionString = connectionString;
        }

        private 
        
        public bool SaveCollection()
        {
            throw new NotImplementedException();
        }

        public bool SaveEntry()
        {
            throw new NotImplementedException();
        }
    }
}
