using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilmBookmarkService.Core
{
    public class WebsiteParserFactory
    {
        private static readonly List<Func<bool, string, IWebsiteParser>> _parsers = new List<Func<bool, string, IWebsiteParser>>
        {
            (useProxy, proxyAddress) => new KinoParser(useProxy, proxyAddress),
        };

        private readonly bool _useProxy;
        private readonly string _proxyAddress;

        public WebsiteParserFactory(bool useProxy, string proxyAddress)
        {
            _useProxy = useProxy;
            _proxyAddress = proxyAddress;
        }

        public Task<IWebsiteParser> CreateParserForUrl(string link)
        {
            return Task.Factory.StartNew(async () =>
            {
                foreach (var creator in _parsers)
                {
                    var parser = creator(_useProxy, _proxyAddress);
                    if (await parser.IsCompatible(link))
                        return parser;
                }

                return null;
            })
            .ContinueWith(result => result.Result.Result);
        }
    }
}