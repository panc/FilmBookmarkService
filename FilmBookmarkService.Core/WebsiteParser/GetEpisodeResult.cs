namespace FilmBookmarkService.Core
{
    public class GetEpisodeResult
    {
        public int Season { get; private set; }
        public int Episode { get; private set; }
        public string StreamUrl { get; private set; }

        public GetEpisodeResult(int season, int episode, string streamUrl)
        {
            Season = season;
            Episode = episode;
            StreamUrl = streamUrl;
        }
    }
}