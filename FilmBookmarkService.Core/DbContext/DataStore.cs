using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FilmBookmarkService.Core
{
    public class DataStore : IDisposable
    {
        private const string FILE_NAME = "films.json";

        private readonly string _filePath;
        private List<Film> _films;

        public DataStore(string appDataPath)
        {
            _filePath = Path.Combine(appDataPath, FILE_NAME);
        }

        public Film[] Films
        {
            get
            {
                _LoadFilmsIfNeeded();
                return _films.ToArray();
            }
        }

        public void AddFilm(Film film)
        {
            _LoadFilmsIfNeeded();

            film.Id = _films.Count;
            _films.Add(film);
        }

        public void RemoveFilm(Film film)
        {
            _LoadFilmsIfNeeded();

            _films.Remove(film);
        }

        public Task SaveChangesAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var content = JsonConvert.SerializeObject(_films);
                File.WriteAllText(_filePath, content);
            });
        }

        private void _LoadFilmsIfNeeded()
        {
            if (_films == null)
            {
                _films = _ReadFilmsFromFile();
            }
        }

        private List<Film> _ReadFilmsFromFile()
        {
            if (!File.Exists(_filePath))
                return new List<Film>();

            var content = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<Film>>(content) ?? new List<Film>();
        }

        public static DataStore Create(string appDataPath)
        {
            return new DataStore(appDataPath);
        }

        public void Dispose()
        {
        }
    }
}