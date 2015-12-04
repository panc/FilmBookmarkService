using System.Threading.Tasks;

namespace FilmBookmarkService.Core
{
    public interface IWebsiteParser
    {
        Task<bool> IsCompatible(string url);

        Task<string> GetStreamUrl(string mirrorLink);

        Task<EpisodeInfo> GetInfoForNextEpisode(string filmUrl, int season, int episode);

        Task<EpisodeInfo> GetInfoForPreviousEpisode(string filmUrl, int season, int episode);
        
        Task<EpisodeInfo> GetEpisodeInfo(string filmUrl, int season, int episode);
    }
}