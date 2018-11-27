using System;
using System.Collections.Generic;
using System.Text;

namespace Ingestion
{
    public class IngestionSource 
    {
        public string name { get; set; }
        public string ApiUrl { get; set; }
        public string Credential { get; set; }
        public string ForwardMessageQueue { get; set; }
    }
}
