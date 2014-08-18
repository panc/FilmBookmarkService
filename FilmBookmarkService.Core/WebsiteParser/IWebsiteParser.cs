using System.Threading.Tasks;

namespace FilmBookmarkService.Core
{
    public interface IWebsiteParser
    {
        Task<bool> IsCompatible(string url);

        Task<string> GetNextStreamUrl(string filmUrl, int season, int episode);
    }
}