using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FilmBookmarkService.Core
{
    public class SettingsStore : IDisposable
    {
        private const string FILE_NAME = "settings.json";

        private static SettingsStore _instance;

        public static SettingsStore Create(string appDataPath)
        {
            if (_instance == null)
                _instance = new SettingsStore(appDataPath);

            return _instance;
        }


        private readonly string _filePath;
        private readonly Lazy<Settings> _settings;

        private SettingsStore(string appDataPath)
        {
            _filePath = Path.Combine(appDataPath, FILE_NAME);
            _settings = new Lazy<Settings>(_ReadSettingsFromFile);
        }

        public Settings Settings => _settings.Value;
        
        public Task SaveChangesAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var content = JsonConvert.SerializeObject(_settings.Value);
                File.WriteAllText(_filePath, content);
            });
        }

        private Settings _ReadSettingsFromFile()
        {
            if (!File.Exists(_filePath))
                return new Settings();

            var content = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<Settings>(content) ?? new Settings();
        }

        public void Dispose()
        {
        }
    }
}