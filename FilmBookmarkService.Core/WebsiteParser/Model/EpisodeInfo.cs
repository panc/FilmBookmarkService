namespace FilmBookmarkService.Core
{
    public class EpisodeInfo
    {
        public int Season { get; private set; }
        public int Episode { get; private set; }
        public MirrorInfo[] Mirrors { get; private set; }
        public int NumberOfEpisodes { get; set; }
        public bool IsAnotherEpisodeAvailable { get; set; }

        public EpisodeInfo(int season, int episode, MirrorInfo[] mirrors, int numberOfEpisodes, bool isAnotherEpisodeAvailable)
        {
            Season = season;
            Episode = episode;
            Mirrors = mirrors;
            NumberOfEpisodes = numberOfEpisodes;
            IsAnotherEpisodeAvailable = isAnotherEpisodeAvailable;
        }
    }
}