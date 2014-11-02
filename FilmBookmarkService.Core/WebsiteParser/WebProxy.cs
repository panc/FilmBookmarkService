namespace FilmBookmarkService.Core
{
    public static class WebProxy
    {
        private const string PROXY_URL = "http://anonymouse.org/cgi-bin/anon-www_de.cgi/";
        
        public static string DecorateUrl(string url)
        {
            return string.Format("{0}{1}", PROXY_URL, url);
        }

        public static string RemoveProxyDecoration(string url)
        {
            return url.Replace(PROXY_URL, string.Empty);
        }
    }
}