namespace FilmBookmarkService.Core
{
    public class GetMirrorResult
    {
        public string Name { get; private set; }
        public int Season { get; private set; }
        public int Episode { get; private set; }
        public string StreamUrl { get; private set; }

        public GetMirrorResult(string name, int season, int episode, string streamUrl)
        {
            Name = name;
            Season = season;
            Episode = episode;
            StreamUrl = streamUrl;
        }
    }
}