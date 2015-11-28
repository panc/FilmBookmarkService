using System.Web;

namespace FilmBookmarkService.Core
{
    public static class WebProxyHelper
    {
        private const string PROXY_URL = "https://webproxy.vpnbook.com/browse.php?u=";
        
        public static string DecorateUrl(string url)
        {
            url = HttpUtility.UrlEncode(url);
            return string.Format("{0}{1}", PROXY_URL, url);
        }

        public static string RemoveProxyDecoration(string url)
        {
            return url.Replace(PROXY_URL, string.Empty);
        }
    }
}