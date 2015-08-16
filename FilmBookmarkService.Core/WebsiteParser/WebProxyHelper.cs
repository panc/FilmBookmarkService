namespace FilmBookmarkService.Core
{
    public static class WebProxyHelper
    {
        //private const string PROXY_URL = "http://anonymouse.org/cgi-bin/anon-www_de.cgi/";
        private const string PROXY_URL = "prx2.unblocksit.es";
        
        public static string DecorateUrl(string url)
        {
            url = url.Replace("http://", "");
            var index = url.IndexOf("/", System.StringComparison.Ordinal);

            if (index < 0)
                return string.Format("{0}.{1}", url, PROXY_URL);

            var prefix = url.Substring(0, index);
            var postfix = url.Substring(index + 1);

            return string.Format("http://{0}.{1}/{2}", prefix, PROXY_URL, postfix);
        }

        public static string RemoveProxyDecoration(string url)
        {
            return url;
            //return url.Replace(PROXY_URL, string.Empty);
        }
    }
}