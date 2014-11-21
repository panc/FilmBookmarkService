using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilmBookmarkService.Core
{
    public static class WebsiteParsingHelper
    {
        private static readonly List<IWebsiteParser> _parsers = new List<IWebsiteParser>
        {
            new KinoxParser(),
        };

        public static Task<IWebsiteParser> GetParser(string link)
        {
            return Task.Factory.StartNew(async () =>
            {
                foreach (var parser in _parsers)
                {
                    if (await parser.IsCompatible(link))
                        return parser;
                }

                return (IWebsiteParser)null;
            })
            .ContinueWith(result => result.Result.Result);
        } 
    }
}