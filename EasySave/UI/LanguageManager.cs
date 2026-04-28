using System.IO;
using System.Text.Json;

namespace EasySave.UI
{
    public class LanguageManager
    {
        private static LanguageManager? _instance;
        private static readonly object _lock = new object();
        private string _currentLanguage = string.Empty;
        private string _resourcesPath;
        private Dictionary<string, string> _translations;

        private LanguageManager() {

            _resourcesPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources"
            );

            _translations = new Dictionary<string, string>();

            LoadLanguage("fr");
        }

        public static LanguageManager GetInstance() {

            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LanguageManager();
                    }
                }
            }
            return _instance;
        }

        public void LoadLanguage(string languageCode) {

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
                _currentLanguage = languageCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERREUR] Impossible de charger la langue : {ex.Message}");
            }
        }

        public string GetText(string key) {

            if (_translations.ContainsKey(key))
            {
                return _translations[key];
            }
            return $"[{key}]";
        }
    }
}
