using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Ingestion
{
    public class IngestionService
    {

        private IList<IngestionSource> _sourcesList;
        private const string _sourcesConfigRelativePath = "config/ingestion_sources.json";

        public IngestionService(IList<IngestionSource> sources)
        {
            _sourcesList = sources;
        }

        public IngestionService()
        {
            _sourcesList = LoadSourcesFromJson();
          
        }

        // Due to sercurity concerns sources are either injected 
        //or loaded from config files
        private IList<IngestionSource> LoadSourcesFromJson() 
        {            
            string json = "";

            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(_sourcesConfigRelativePath))
            {
                
            }

            List<string> videogames = JsonConvert.DeserializeObject<List<string>>(json);

            Console.WriteLine(string.Join(", ", videogames.ToArray()));
            return null;
        }

        public void Ingest(int sourceID) {
            foreach (IngestionSource source in _sourcesList) {
                
            }

            PutInMessageQueue();

        }

        private void PutInMessageQueue()
        {
            throw new NotImplementedException();
        }
    }
}
