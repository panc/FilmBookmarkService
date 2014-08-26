using System.Threading.Tasks;

namespace FilmBookmarkService.Core
{
    public interface IWebsiteParser
    {
        Task<bool> IsCompatible(string url);

        Task<string> GetStreamUrl(string filmUrl, int season, int episode);

        Task<GetEpisodeResult> GetNextEpisode(string filmUrl, int season, int episode);

        Task<GetEpisodeResult> GetPrevEpisode(string filmUrl, int season, int episode);
        
        Task<bool> IsAnotherEpisodeAvailable(string filmUrl, int season, int episode);
    }
}