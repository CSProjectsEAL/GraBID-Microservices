using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Refiner
{
    public class DataHandler
    {
        private IDataProcessor _processor;
        private IDBFacade _db;
        public string Destination { get; private set; }
        public string CollectionName { get; private set; }
        public string Database { get; private set; }
        public DataHandler(IDataProcessor processor, string destination, IDBFacade db = null)
        {
            _processor = processor;
            _db = db;
        }

        public void Handle(string data)
        {
            string processedData = _processor.Process(data);

            if (_db != null) {
                _db.SaveCollection(processedData, CollectionName, Database);
            }
        }

    }
}
