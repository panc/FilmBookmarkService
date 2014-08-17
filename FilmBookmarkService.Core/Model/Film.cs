using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmBookmarkService.Core
{
    public class Film
    {
        private IWebsiteParser _parser;

        // http://www.asp.net/mvc/tutorials/getting-started-with-ef-using-mvc/creating-an-entity-framework-data-model-for-an-asp-net-mvc-application

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Link { get; set; }

        public string LinkIdPart { get; set; }
        
        public string Stream { get; set; }
        
        public int Season { get; set; }
        
        public int Episode { get; set; }
        
        public string ParserType { get; set; }

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