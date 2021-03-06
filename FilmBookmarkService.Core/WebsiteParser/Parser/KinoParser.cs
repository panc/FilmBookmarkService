﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FilmBookmarkService.Core.CloudFlareUtilities;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace FilmBookmarkService.Core
{
    public class KinoParser : IWebsiteParser
    {
        private const string DOMAIN_NAME = "kino" + "x";
        private const string BASE_URL = DOMAIN_NAME + ".sg";
        private const string GET_HOSTER_URL = "http://" + BASE_URL + "/aGET/MirrorByEpisode/?Addr={0}&Season={1}&Episode={2}";
        private const string GET_URL = "http://" + BASE_URL + "/aGET/Mirror/{0}";
        private const string URL_TEMPLATE = BASE_URL + "/Stream/";

        private static readonly string[] Domains = { "ag", "am", "me", "nu", "pe", "sg", "to", "tv" };

        private readonly bool _useProxy;
        private readonly string _proxyAddress;

        public KinoParser(bool useProxy, string proxyAddress)
        {
            _useProxy = useProxy;
            _proxyAddress = proxyAddress;
        }

        public Task<bool> IsCompatible(string url)
        {
            return Task.Factory.StartNew(() =>
            {
                url = _PrepareUrl(url);
                url = _RemoveProtocol(url);

                return !string.IsNullOrEmpty(url) && url.StartsWith(URL_TEMPLATE);
            });
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
                // ignored
            }

            if (mirror == null)
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(mirror.Stream);
            var node = doc.DocumentNode.SelectSingleNode("//a[@href]");

            var mirrorUrl = node.Attributes["href"].Value;

            var httpIndex = mirrorUrl.IndexOf("http://", StringComparison.InvariantCulture);
            if (httpIndex > 0)
                return mirrorUrl.Substring(httpIndex);

            var httpsIndex = mirrorUrl.IndexOf("https://", StringComparison.InvariantCulture);
            if (httpsIndex > 0)
                return mirrorUrl.Substring(httpsIndex);

            return mirrorUrl;
        }

        public async Task<EpisodeInfo> GetEpisodeInfo(string filmUrl, int season, int episode)
        {
            var mirrors = await _GetMirrors(filmUrl, season, episode);

            return await _GetEpisodeInfo(filmUrl, season, episode, mirrors);
        }

        public async Task<EpisodeInfo> GetInfoForNextEpisode(string filmUrl, int season, int episode)
        {
            episode++;
            var mirrors = await _GetMirrors(filmUrl, season, episode);

            if (mirrors == null || mirrors.Length == 0)
            {
                // We reached the end of the season.
                // Try to get the first episode for the next season.
                episode = 1;
                season++;

                mirrors = await _GetMirrors(filmUrl, season, episode);

                if (mirrors == null)
                    return null;
            }

            return await _GetEpisodeInfo(filmUrl, season, episode, mirrors);
        }

        public async Task<EpisodeInfo> GetInfoForPreviousEpisode(string filmUrl, int season, int episode)
        {
            episode--;

            if (episode == 0)
                throw new Exception("Start of season reached! Jumping to the previous season is not supported yet!");

            var mirrors = await _GetMirrors(filmUrl, season, episode);

            if (mirrors == null)
                return null;

            return await _GetEpisodeInfo(filmUrl, season, episode, mirrors);
        }

        private async Task<EpisodeInfo> _GetEpisodeInfo(string filmUrl, int season, int episode, MirrorInfo[] mirrors)
        {
            var seasonSelectionNode = await _GetSeasonSelectionNode(filmUrl);

            var numberOfEpisodes = _GetNumberOfEpisodes(seasonSelectionNode, season);
            var isEpisodeAvailable = _CheckIsEpisodeAvailable(seasonSelectionNode, season, episode);

            return new EpisodeInfo(season, episode, mirrors, numberOfEpisodes, isEpisodeAvailable);
        }

        private async Task<MirrorInfo[]> _GetMirrors(string filmUrl, int season, int episode)
        {
            var filmId = await _ParseUrlForFilmId(filmUrl);
            var url = string.Format(GET_HOSTER_URL, filmId, season, episode);

            var content = await _ExecuteHttpRequest(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var mirrorNodes = doc.DocumentNode.SelectNodes("//li[starts-with(@id, 'Hoster_')]");

            if (mirrorNodes == null)
                return new MirrorInfo[0];

            var mirrors = new List<MirrorInfo>();

            foreach (var node in mirrorNodes)
            {
                var mirrorsForHoster = _GetMirrorsForHoster(node, season, episode);
                mirrors.AddRange(mirrorsForHoster);
            }

            return mirrors.ToArray();
        }

        private IEnumerable<MirrorInfo> _GetMirrorsForHoster(HtmlNode node, int season, int episode)
        {
            var name = node.SelectSingleNode("div[@class='Named']").InnerHtml;

            var rel = node.GetAttributeValue("rel", "");
            var mirrorIndexInLink = rel.IndexOf("Mirror=", StringComparison.InvariantCulture);

            // prepare link so that we only have to replace the mirror number
            var link = (mirrorIndexInLink < 0)
                ? rel
                : rel.Substring(0, mirrorIndexInLink) + "Mirror={0}" + rel.Substring(mirrorIndexInLink + 8);

            // get number of mirrors for this hoster
            var mirrorInfo = node.SelectSingleNode("div[@class='Data']/text()[1]").InnerHtml; // e.g.: ": 1/2"

            var separatorIndex = mirrorInfo.IndexOf("/", StringComparison.InvariantCulture);
            int count;

            int.TryParse(mirrorInfo.Substring(separatorIndex + 1), out count);

            for (int i = 1; i <= count; i++)
            {
                var l = string.Format(link, i);
                var n = $"{name} {i}";
                yield return new MirrorInfo(n, season, episode, l);
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

        private bool _CheckIsEpisodeAvailable(HtmlNode seasonSelectionNode, int season, int episode)
        {
            var isEpisodeAvailable = _CheckNodeForEpisodeOption(seasonSelectionNode, season, episode + 1);

            if (!isEpisodeAvailable)
                return _CheckNodeForEpisodeOption(seasonSelectionNode, season + 1, 1);

            return true;
        }

        private bool _CheckNodeForEpisodeOption(HtmlNode seasonSelectionNode, int season, int episode)
        {
            var episodes = _GetEpisodes(seasonSelectionNode, season);

            return episodes.Contains(episode.ToString(CultureInfo.InvariantCulture));
        }

        private int _GetNumberOfEpisodes(HtmlNode seasonSelectionNode, int season)
        {
            var lastEpisode = _GetEpisodes(seasonSelectionNode, season).Last();
            return Convert.ToInt32(lastEpisode);
        }

        private IEnumerable<string> _GetEpisodes(HtmlNode seasonSelectionNode, int season)
        {
            var optionNode = seasonSelectionNode.SelectSingleNode($"//option[@value='{season}']");

            if (optionNode == null)
                return new string[0];

            var episodes = optionNode
                .GetAttributeValue("rel", "0")
                .Split(',');

            return episodes;
        }

        private async Task<string> _ParseUrlForFilmId(string url)
        {
            if (!await IsCompatible(url))
                return string.Empty;

            url = _RemoveProtocol(url);

            return _PrepareUrl(url).Replace(URL_TEMPLATE, "").Replace(".html", "");
        }

        private string _PrepareUrl(string url)
        {
            foreach (var domain in Domains)
            {
                url = url.Replace($"{DOMAIN_NAME}.{domain}", BASE_URL);
            }

            return url;
        }

        private static string _RemoveProtocol(string url)
        {
            return url.Replace("http://www.", "")
                .Replace("http://", "")
                .Replace("https://www.", "")
                .Replace("https://", "");
        }

        private async Task<string> _ExecuteHttpRequest(string filmUrl)
        {
            //            var client = !_useProxy
            //            ? new HttpClient()
            //            : new HttpClient(new HttpClientHandler
            //            {
            //                UseProxy = true,
            //                Proxy = new WebProxy(_proxyAddress, false)
            //            });


            // Create the clearance handler.
            var handler = new ClearanceHandler
            {
                MaxRetries = 2 // Optionally specify the number of retries, if clearance fails (default is 3).
            };

            // Create a HttpClient that uses the handler to bypass CloudFlare's JavaScript challange.
            var client = new HttpClient(handler);

            var url = _PrepareUrl(filmUrl);

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
    }
}