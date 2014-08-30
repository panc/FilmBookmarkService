using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FilmBookmarkService.Core
{
    public class FilmStore : IDisposable
    {
        private const string FILE_NAME = "films.json";

        private readonly string _filePath;
        private readonly Lazy<List<Film>> _films;

        public FilmStore(string appDataPath)
        {
            _films = new Lazy<List<Film>>(_ReadFilmsFromFile);
            _filePath = Path.Combine(appDataPath, FILE_NAME);
        }

        public Film[] Films
        {
            get { return _films.Value.ToArray(); }
        }

        public void AddFilm(Film film)
        {
            film.Id = _films.Value.Count;
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
                var content = JsonConvert.SerializeObject(_films);
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

        public static FilmStore Create(string appDataPath)
        {
            return new FilmStore(appDataPath);
        }

        public void Dispose()
        {
        }
    }
}