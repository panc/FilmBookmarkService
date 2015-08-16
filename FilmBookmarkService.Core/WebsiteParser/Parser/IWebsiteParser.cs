using System.Threading.Tasks;

namespace FilmBookmarkService.Core
{
    public interface IWebsiteParser
    {
        Task<bool> IsCompatible(string url);

        Task<string> GetStreamUrl(string filmUrl, string url);

        Task<int> GetNumberOfEpisodes(string filmUrl, int season);

        Task<GetEpisodeResult> GetNextEpisode(string filmUrl, int season, int episode);

        Task<GetEpisodeResult> GetPrevEpisode(string filmUrl, int season, int episode);
        
        Task<bool> IsAnotherEpisodeAvailable(string filmUrl, int season, int episode);
        
        Task<GetMirrorResult[]> GetMirrors(string url, int season, int episode);
    }
}