namespace FilmBookmarkService.Core
{
    public static class WebProxy
    {
        private const string PROXY_URL = "http://anonymouse.org/cgi-bin/anon-www_de.cgi/{0}";

        public static string DecorateUrl(string url)
        {
            return string.Format(PROXY_URL, url);
        }
    }
}