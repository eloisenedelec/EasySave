namespace EasySave.UI
{
    public class LanguageManager
    {
        private static LanguageManager _instance;
        private string _currentLanguage;
        private Dictionary<string, string> _translations;

        private LanguageManager() {

            _resourcesPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources"
            );

            _translations = new Dictionary<string, string>();

            LoadLanguage("fr"); // fr par défaut
        }

        public static LanguageManager GetInstance() {

            if (_instance == null)
            {
                lock (_lock) // verrou temp en cas de multi thread
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

            string fileName = $"lang_{languageCode}.json";
            string filePath = Path.Combine(_resourcesPath, $"lang_{languageCode}.json");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[ERREUR] Fichier de langue introuvable : {filePath}");
                return;
            }
            try
            {
                string json = File.ReadAllText(filePath); // convert en str
                _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json); // convert en objet dico clé valeur
                _currentLanguage = languageCode;

                Console.WriteLine($"[OK] Langue chargée : {languageCode}");
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
            return $"[{key}]"; // retourne la clé si pas de valeur

        } 
    }
}
