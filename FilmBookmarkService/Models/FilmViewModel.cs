namespace FilmBookmarkService
{
    public class FilmViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string CoverUrl { get; set; }

        public int Season { get; set; }

        public int Episode { get; set; }

        public bool IsFavorite { get; set; }
    }
}