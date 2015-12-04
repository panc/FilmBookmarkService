namespace FilmBookmarkService.Core
{
    public class MirrorInfo
    {
        public string Name { get; private set; }
        public int Season { get; private set; }
        public int Episode { get; private set; }
        public string StreamUrl { get; private set; }

        public MirrorInfo(string name, int season, int episode, string streamUrl)
        {
            Name = name;
            Season = season;
            Episode = episode;
            StreamUrl = streamUrl;
        }
    }
}