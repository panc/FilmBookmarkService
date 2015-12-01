using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace FilmBookmarkService.Core
{
    public class KinoParser : IWebsiteParser
    {
        private const string GET_HOSTER_URL = "http://kino" + "x.tv/aGET/MirrorByEpisode/?Addr={0}&Season={1}&Episode={2}";
        private const string GET_URL = "http://kino" + "x.tv/aGET/Mirror/{0}";
        private const string LOCKED_BASE_URL = "kino" + "x.to";
        private const string BASE_URL = "kino" + "x.tv";
        private const string URL_TEMPLATE = "kino" + "x.tv/Stream/";

        public Task<bool> IsCompatible(string url)
        {
            return Task.Factory.StartNew(() =>
            {
                url = _PrepareUrl(url);
                url = url.Replace("http://www.", "")
                         .Replace("http://", "");

                return !string.IsNullOrEmpty(url) && url.StartsWith(URL_TEMPLATE);
            });
        }

        public async Task<int> GetNumberOfEpisodes(string filmUrl, int season)
        {
            var seasonSelectionNode = await _GetSeasonSelectionNode(filmUrl);
            var numberOfEpisodes = _GetEpisodes(seasonSelectionNode, season).Last();

            return Convert.ToInt32(numberOfEpisodes);
        }

        public async Task<string> GetStreamUrl(string mirrorLink)
        {
            var url = string.Format(GET_URL, HttpUtility.UrlDecode(mirrorLink.Replace("&amp;", "&")));

            MirrorDto mirror = null;
            var content = await _ExecuteHttpRequest(url);
            
            try
            {
                mirror = JsonConvert.DeserializeObject<MirrorDto>(content);
            }
            catch
            {
            }

            if (mirror == null)
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(mirror.Stream);
            var node = doc.DocumentNode.SelectSingleNode("//a[@href]");

            var mirrorUrl = node.Attributes["href"].Value;

            var httpIndex = mirrorUrl.IndexOf("http://");
            if (httpIndex > 0)
                mirrorUrl = mirrorUrl.Substring(httpIndex);

            return mirrorUrl;
        }

        public async Task<GetMirrorResult[]> GetMirrors(string filmUrl, int season, int episode)
        {
            var filmId = await _ParseUrlForFilmId(filmUrl);
            var url = string.Format(GET_HOSTER_URL, filmId, season, episode);

            var content = await _ExecuteHttpRequest(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var mirrorNodes = doc.DocumentNode.SelectNodes("//li[starts-with(@id, 'Hoster_')]");

            if (mirrorNodes == null)
                return new GetMirrorResult[0];

            var mirrors = new List<GetMirrorResult>();

            foreach (var node in mirrorNodes)
            {
                var mirrorsForHoster = _GetMirrorsForHoster(node, season, episode);
                mirrors.AddRange(mirrorsForHoster);
            }

            return mirrors.ToArray();
        }

        public async Task<GetEpisodeResult> GetNextEpisode(string filmUrl, int season, int episode)
        {
            episode++;
            var mirrors = await GetMirrors(filmUrl, season, episode);

            if (mirrors == null || mirrors.Length == 0)
            {
                // We reached the end of the season.
                // Try to get the first episode for the next season.
                episode = 1;
                season++;

                mirrors = await GetMirrors(filmUrl, season, episode);

                if (mirrors == null)
                    return null;
            }

            return new GetEpisodeResult(season, episode, mirrors);
        }

        public async Task<GetEpisodeResult> GetPrevEpisode(string filmUrl, int season, int episode)
        {
            episode--;

            if (episode == 0)
                throw new Exception("Start of season reached! Jumping to the previous season is not supported yet!");

            var mirror = await GetMirrors(filmUrl, season, episode);

            if (mirror == null)
                return null;

            return new GetEpisodeResult(season, episode, mirror);
        }

        public async Task<bool> IsAnotherEpisodeAvailable(string filmUrl, int season, int episode)
        {
            var seasonSelectionNode = await _GetSeasonSelectionNode(filmUrl);
            var isEpisodeAvailable = _CheckNodeForEpisodeOption(seasonSelectionNode, season, episode + 1);

            if (!isEpisodeAvailable)
                return _CheckNodeForEpisodeOption(seasonSelectionNode, season + 1, 1);

            return true;
        }

        private IEnumerable<GetMirrorResult> _GetMirrorsForHoster(HtmlNode node, int season, int episode)
        {
            var name = node.SelectSingleNode("div[@class='Named']").InnerHtml;

            var rel = node.GetAttributeValue("rel", "");
            var mirrorIndexInLink = rel.IndexOf("Mirror=");

            // prepare link so that we only have to replace the mirror number
            var link = (mirrorIndexInLink < 0)
                ? rel
                : rel.Substring(0, mirrorIndexInLink) + "Mirror={0}" + rel.Substring(mirrorIndexInLink + 8);

            // get number of mirrors for this hoster
            var mirrorInfo = node.SelectSingleNode("div[@class='Data']/text()[1]").InnerHtml; // e.g.: ": 1/2"

            var separatorIndex = mirrorInfo.IndexOf("/");
            var count = 0;

            int.TryParse(mirrorInfo.Substring(separatorIndex + 1), out count);

            for (int i = 1; i <= count; i++)
            {
                var l = string.Format(link, i);
                var n = $"{name} {i}";
                yield return new GetMirrorResult(n, season, episode, l);
            }
        }

        private async Task<HtmlNode> _GetSeasonSelectionNode(string filmUrl)
        {
            var content = await _ExecuteHttpRequest(filmUrl);

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var seasonSelectionNode = doc.DocumentNode.SelectSingleNode("//select[@id='SeasonSelection']");

            if (seasonSelectionNode == null)
                seasonSelectionNode = doc.DocumentNode.SelectSingleNode("//select[@id='season_select']");

            return seasonSelectionNode;
        }

        private bool _CheckNodeForEpisodeOption(HtmlNode seasonSelectionNode, int season, int episode)
        {
            var episodes = _GetEpisodes(seasonSelectionNode, season);

            return episodes.Contains(episode.ToString(CultureInfo.InvariantCulture));
        }

        private IEnumerable<string> _GetEpisodes(HtmlNode seasonSelectionNode, int season)
        {
            var optionNode = seasonSelectionNode.SelectSingleNode(string.Format("//option[@value='{0}']", season));

            var episodes = optionNode
                .GetAttributeValue("rel", "1")
                .Split(',');

            return episodes;
        }

        private async Task<string> _ParseUrlForFilmId(string url)
        {
            if (!await IsCompatible(url))
                return string.Empty;

            url = url.Replace("http://www.", "")
                     .Replace("http://", "");

            return _PrepareUrl(url).Replace(URL_TEMPLATE, "").Replace(".html", "");
        }

        private string _PrepareUrl(string url)
        {
            return url.Replace(LOCKED_BASE_URL, BASE_URL);
        }

        private async Task<string> _ExecuteHttpRequest(string filmUrl)
        {
            //filmUrl = WebProxyHelper.DecorateUrl(filmUrl);

            var handler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = new WebProxy("http://108.162.206.233", false)
            };

            var url = _PrepareUrl(filmUrl);
            var client = new HttpClient(handler);
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
    }
}