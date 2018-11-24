using System;
using System.Collections.Generic;
using System.Text;

namespace Ingestion
{
    public class IngestionSource 
    {
        public int id { get; set; }
        public string ApiUrl { get; set; }
        public string Credential { get; set; }
        public string ForwardMessageQueue { get; set; }
    }
}
