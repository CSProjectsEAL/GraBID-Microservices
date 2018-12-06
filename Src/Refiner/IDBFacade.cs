using System;
using System.Collections.Generic;
using System.Text;

namespace Refiner
{
    //Wrapper class to make DB layer abstract
    public interface IDBFacade
    {
        void SaveCollection(string collection, string collectionName, string databaseName);
        string FetchCollection(string id, string collectionName, string databaseName);
    }
}
