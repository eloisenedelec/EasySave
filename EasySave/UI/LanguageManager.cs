using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace EasySave.UI
{
    public class LanguageManager : INotifyPropertyChanged
    {
        private static LanguageManager? _instance;
        private static readonly object _lock = new object();
        private string _resourcesPath;
        private Dictionary<string, string> _translations;

        public static LanguageManager Instance => GetInstance();

        private LanguageManager()
        {
            _resourcesPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources"
            );
            _translations = new Dictionary<string, string>();
            LoadLanguage("fr");
        }

        public static LanguageManager GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new LanguageManager();
                }
            }
            return _instance;
        }

        public void LoadLanguage(string languageCode)
        {
            string filePath = Path.Combine(_resourcesPath, $"lang_{languageCode}.json");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[ERREUR] Fichier de langue introuvable : {filePath}");
                return;
            }
            try
            {
                string json = File.ReadAllText(filePath);
                _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERREUR] Impossible de charger la langue : {ex.Message}");
            }
        }

        public string this[string key] => _translations.TryGetValue(key, out var val) ? val : $"[{key}]";

        public string GetText(string key) => this[key];

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
