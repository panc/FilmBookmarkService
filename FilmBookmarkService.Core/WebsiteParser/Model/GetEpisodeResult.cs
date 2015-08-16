namespace FilmBookmarkService.Core
{
    public class GetEpisodeResult
    {
        public int Season { get; private set; }
        public int Episode { get; private set; }
        public GetMirrorResult[] Mirrors { get; private set; }

        public GetEpisodeResult(int season, int episode, GetMirrorResult[] mirrors)
        {
            Season = season;
            Episode = episode;
            Mirrors = mirrors;
        }
    }
}