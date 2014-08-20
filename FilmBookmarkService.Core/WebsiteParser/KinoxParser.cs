using System;
using System.Net;
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

        public async Task<string> GetStreamUrl(string filmUrl, int season, int episode)
        {
            var mirror = await _GetMirror(filmUrl, season, episode, HOSTER_STREAMCLOUD);

            var doc = new HtmlDocument();
            doc.LoadHtml(mirror.Stream);
            var node = doc.DocumentNode.SelectSingleNode("//a[@href]");

            return node.Attributes["href"].Value;
        }

        public async Task<GetEpisodeResult> GetNextEpisode(string filmUrl, int season, int episode)
        {
            episode++;
            var mirror = await _GetMirror(filmUrl, season, episode, HOSTER_STREAMCLOUD);

            if (mirror == null)
            {
                // We reached the end of the season.
                // Try to get the first episode for the next season.
                episode = 1;
                season++;

                mirror = await _GetMirror(filmUrl, season, episode, HOSTER_STREAMCLOUD);

                if (mirror == null)
                    return null;
            }

            return new GetEpisodeResult(season, episode, mirror.Stream);
        }

        public async Task<GetEpisodeResult> GetPrevEpisode(string filmUrl, int season, int episode)
        {
            episode--;

            if (episode == 0)
                throw new Exception("Start of season reached! Jumping to the previous season is not supported yet!");

            var mirror = await _GetMirror(filmUrl, season, episode, HOSTER_STREAMCLOUD);

            if (mirror == null)
                return null;
            
            return new GetEpisodeResult(season, episode, mirror.Stream);
        }

        private async Task<KinoxMirrorDto> _GetMirror(string filmUrl, int season, int episode, string hoster)
        {
            var filmId = _ParseUrlForFilmId(filmUrl);
            var url = string.Format(GET_MIRROR_URL, filmId, hoster, season, episode);

//            var handler = new HttpClientHandler
//            {
//                UseDefaultCredentials = false,
//                Proxy = new WebProxy("ap-proxy", 8080) { Credentials = new NetworkCredential("", "") },
//                UseProxy = true,
//            };

            var client = new HttpClient();
            var response = await client.GetAsync(new Uri(url));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KinoxMirrorDto>(content);
        }

        private string _ParseUrlForFilmId(string url)
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith(BASE_URL))
                return string.Empty;

            return url.Replace(BASE_URL, "").Replace(".html", "");
        }
    }
}