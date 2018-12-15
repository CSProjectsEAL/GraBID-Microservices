using Serilog;
using Shared;
using System.Collections.Generic;

namespace Refiner
{
    public class DataSource
    {
        public string Topic { get; set; }
        public string Exchange { get; set; }
        public IList<DataHandler> DataHandlers {get;set;}
        public void Process(Envelope<string> envelope)
        {
            foreach (DataHandler d in DataHandlers)
            {
                string data = d.Process(envelope.Payload);
                
            }
            Log.Information("Data successfully processed");
        }
    }
}
