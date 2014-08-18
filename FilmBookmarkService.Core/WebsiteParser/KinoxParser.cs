using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace FilmBookmarkService.Core
{
    public class KinoxParser : IWebsiteParser
    {
        // Sample URL
        // http://kinox.to/Stream/Castle.html

        private const string HOSTER_STREAMCLOUD = "30";
        private const string GET_MIRROR_URL = "http://kinox.to/aGET/Mirror/{0}&Hoster={1}&Season={2}&Episode={3}";
        private const string BASE_URL = "http://kinox.to/Stream/";

        public Task<bool> IsCompatible(string url)
        {
            return Task.Factory.StartNew(() => !string.IsNullOrEmpty(url) && url.StartsWith(BASE_URL));
        }

        public async Task<string> GetNextStreamUrl(string filmUrl, int season, int episode)
        {
            var filmId = _ParseUrlForFilmId(filmUrl);
            var url = string.Format(GET_MIRROR_URL, filmId, HOSTER_STREAMCLOUD, season, episode);

            var client = new HttpClient();
            var response = await client.GetAsync(new Uri(url));
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var dto = JsonConvert.DeserializeObject<KinoxMirrorDto>(content);

            var doc = new HtmlDocument();
            doc.LoadHtml(dto.Stream);
            var node = doc.DocumentNode.SelectSingleNode("//a[@href]");

            return node.Attributes["href"].Value;
        }

        private string _ParseUrlForFilmId(string url)
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith(BASE_URL))
                return string.Empty;

            return url.Replace(BASE_URL, "").Replace(".html", "");
        }
    }
}