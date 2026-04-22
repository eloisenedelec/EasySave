namespace EasySave.UI
{
    public class LanguageManager
    {
        private static LanguageManager _instance;
        private string _currentLanguage;
        private Dictionary<string, string> _translations;

        private LanguageManager() { // TODO }

        public static LanguageManager GetInstance() { throw new NotImplementedException(); } // TODO
        public void LoadLanguage(string languageCode) { // TODO }
        public string GetText(string key) { throw new NotImplementedException(); } // TODO
        public void ChangeLanguage(string languageCode) { // TODO }
    }
}
