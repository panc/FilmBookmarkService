using System;
using System.Globalization;
using System.Linq;
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
        private const string GET_MIRROR_URL = "http://kinox.to/aGET/Mirror/{0}&Hoster={1}&Season={2}&Episode={3}&Mirror={4}";
        private const string BASE_URL = "kinox.to/Stream/";

        public Task<bool> IsCompatible(string url)
        {
            return Task.Factory.StartNew(() =>
            {
                url = _PrepareBaseUrl(url);
                return !string.IsNullOrEmpty(url) && url.StartsWith(BASE_URL);
            });
        }

        public async Task<string> GetCoverUrl(string filmUrl)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(new Uri(filmUrl));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var coverNode = doc.DocumentNode.SelectSingleNode("//div[@class='Grahpics']//a//img[@src]");

            return coverNode != null
                ? coverNode.GetAttributeValue("src", "")
                : string.Empty;
        }

        public async Task<int> GetNumberOfEpisodes(string filmUrl, int season)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(new Uri(filmUrl));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var seasonSelectionNode = doc.DocumentNode.SelectSingleNode("//select[@id='SeasonSelection']");
            var seasonNode = seasonSelectionNode.SelectSingleNode(string.Format("//option[@value='{0}']", season));

            var numberOfEpisodes = seasonNode
                .GetAttributeValue("rel", "")
                .Split(',')
                .Last();

            return Convert.ToInt32(numberOfEpisodes);
        }

        public async Task<string> GetStreamUrl(string filmUrl, int season, int episode)
        {
            return await _GetMirror(filmUrl, season, episode, HOSTER_STREAMCLOUD);
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

            return new GetEpisodeResult(season, episode, mirror);
        }

        public async Task<GetEpisodeResult> GetPrevEpisode(string filmUrl, int season, int episode)
        {
            episode--;

            if (episode == 0)
                throw new Exception("Start of season reached! Jumping to the previous season is not supported yet!");

            var mirror = await _GetMirror(filmUrl, season, episode, HOSTER_STREAMCLOUD);

            if (mirror == null)
                return null;

            return new GetEpisodeResult(season, episode, mirror);
        }

        public async Task<bool> IsAnotherEpisodeAvailable(string filmUrl, int season, int episode)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(new Uri(filmUrl));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var seasonSelectionNode = doc.DocumentNode.SelectSingleNode("//select[@id='SeasonSelection']");

            var isEpisodeAvailable = _CheckNodeForEpisodeOption(seasonSelectionNode, season, episode + 1);

            if (!isEpisodeAvailable)
                return _CheckNodeForEpisodeOption(seasonSelectionNode, season + 1, 1);

            return true;
        }

        private bool _CheckNodeForEpisodeOption(HtmlNode seasonSelectionNode, int season, int episode)
        {
            var optionNode = seasonSelectionNode.SelectSingleNode(string.Format("//option[@value='{0}']", season));

            var episodes = optionNode
                .GetAttributeValue("rel", "")
                .Split(',');

            return episodes.Contains(episode.ToString(CultureInfo.InvariantCulture));
        }

        private async Task<string> _GetMirror(string filmUrl, int season, int episode, string hoster)
        {
            // We assume that 5 mirrors are available
            // so we are trying to get the link to one of those 5.
            // It's no problem if the mirror is not available, 
            // the the service returns the default mirror (number 1).
            var mirrorNumber = new Random().Next(1, 5);

            var filmId = _ParseUrlForFilmId(filmUrl);
            var url = string.Format(GET_MIRROR_URL, filmId, hoster, season, episode, mirrorNumber);

            var client = new HttpClient();
            var response = await client.GetAsync(new Uri(url));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var mirror = JsonConvert.DeserializeObject<KinoxMirrorDto>(content);

            if (mirror == null)
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(mirror.Stream);
            var node = doc.DocumentNode.SelectSingleNode("//a[@href]");

            return node.Attributes["href"].Value;
        }

        private string _ParseUrlForFilmId(string url)
        {
            url = _PrepareBaseUrl(url);

            if (string.IsNullOrEmpty(url) || !url.StartsWith(BASE_URL))
                return string.Empty;

            return url.Replace(BASE_URL, "").Replace(".html", "");
        }

        private string _PrepareBaseUrl(string url)
        {
            return url.Replace("http://www.", "").Replace("http://", "");
        }
    }
}