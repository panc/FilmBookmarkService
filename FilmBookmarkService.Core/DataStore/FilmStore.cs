using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FilmBookmarkService.Core
{
    public class FilmStore : IDisposable
    {
        private const string FILE_NAME = "films.{0}.json";

        private static readonly Dictionary<string, FilmStore> _cache = new Dictionary<string, FilmStore>(); 

        public static FilmStore Create(string appDataPath, string postFix)
        {
            FilmStore store;
            if (_cache.TryGetValue(postFix, out store))
                return store;
            
            store = new FilmStore(appDataPath, postFix);
            _cache.Add(postFix, store);

            return store;
        }


        private readonly string _filePath;
        private readonly Lazy<List<Film>> _films;

        private FilmStore(string appDataPath, string postFix)
        {
            _films = new Lazy<List<Film>>(_ReadFilmsFromFile);
            _filePath = Path.Combine(appDataPath, string.Format(FILE_NAME, postFix));
        }

        public Film[] Films => _films.Value.ToArray();

        public void AddFilm(Film film)
        {
            film.Id = _films.Value.Count > 0 
                ? _films.Value.Max(x => x.Id) + 1
                : 1;

            film.SortIndex = film.Id + 1;
            _films.Value.Add(film);
        }

        public void RemoveFilm(Film film)
        {
            _films.Value.Remove(film);
        }

        public Task SaveChangesAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var content = JsonConvert.SerializeObject(_films.Value);
                File.WriteAllText(_filePath, content);
            });
        }

        private List<Film> _ReadFilmsFromFile()
        {
            if (!File.Exists(_filePath))
                return new List<Film>();

            var content = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<Film>>(content) ?? new List<Film>();
        }

        public void Dispose()
        {
        }
    }
}