using System;
using System.Collections.Generic;
using System.Text;

namespace Refiner
{
    public class ResellerProcessor : IDataProcessor
    {
        private string data;
        private string 
        public ResellerProcessor(string outputQueueName, MongoDBConnector mongoDB, string database)
        {

        }


        private string Process(string data)
        {
            string extractedData = Extract(data);
            string transformedData = Transform(data);
            Publish(data);
        }

        public string CleanseData(dynamic data) {

          

            return ;
        }

        private string Extract(string data)
        {
            
        }

        private string Transform()
        {

        }


        private string Transform()
        {

        }

        private string Publish()
        {

        }

    }
}