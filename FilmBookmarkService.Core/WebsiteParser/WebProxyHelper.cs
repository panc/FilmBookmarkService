using System.Web;

namespace FilmBookmarkService.Core
{
    public static class WebProxyHelper
    {
        private const string PROXY_URL = "https://webproxy.vpnbook.com/browse.php?u=";
        
        public static string DecorateUrl(string url)
        {
            url = HttpUtility.UrlEncode(url);
            return $"{PROXY_URL}{url}";
        }

        public static string RemoveProxyDecoration(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            return url.Replace(PROXY_URL, string.Empty);
        }
    }
}