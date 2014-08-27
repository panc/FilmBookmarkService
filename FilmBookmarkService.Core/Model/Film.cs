using System;
using Newtonsoft.Json;

namespace FilmBookmarkService.Core
{
    public class Film
    {
        private IWebsiteParser _parser;

        public int Id { get; set; }
        
        public int SortIndex { get; set; }
        
        public string Name { get; set; }
        
        public string Url { get; set; }
        
        public int Season { get; set; }
        
        public int Episode { get; set; }
        
        public string ParserType { get; set; }

        [JsonIgnore]
        public IWebsiteParser Parser
        {
            get
            {
                if (_parser == null)
                {
                    var type = Type.GetType(ParserType);
                    _parser = Activator.CreateInstance(type) as IWebsiteParser;
                }

                return _parser;
            }
        }

        public void SetParser(IWebsiteParser parser)
        {
            ParserType = parser.GetType().FullName;
        }
    }
}