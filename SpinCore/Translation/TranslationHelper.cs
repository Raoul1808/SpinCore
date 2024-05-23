using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SpinCore.Translation
{
    public static class TranslationHelper
    {
        private static bool _readyToAdd = false;
        private static void MarkReadyToAdd() => _readyToAdd = true;

        private static Dictionary<string, TranslatedString> _pendingTranslations = new Dictionary<string, TranslatedString>();

        public static void LoadTranslationsFromStream(Stream stream)
        {
            string fullText = "";
            using (StreamReader reader = new StreamReader(stream))
            {
                fullText = reader.ReadToEnd();
            }

            var strings = JsonConvert.DeserializeObject<Dictionary<string, TranslatedString>>(fullText);
            foreach (var stringPair in strings)
            {
                AddTranslation(stringPair.Key, stringPair.Value);
            }
        }

        public static void AddTranslation(string key, TranslatedString value)
        {
            if (!_readyToAdd)
            {
                _pendingTranslations.Add(key, value);
                return;
            }

            AddKey(key, value);
        }

        public static void RemoveTranslation(string key)
        {
            if (!_readyToAdd)
            {
                _pendingTranslations.Remove(key);
                return;
            }

            RemoveKey(key);
        }

        internal static void AddAllPendingKeys()
        {
            MarkReadyToAdd();
            foreach (var pair in _pendingTranslations)
            {
                AddKey(pair.Key, pair.Value);
            }
            _pendingTranslations.Clear();
        }
        
        private static void AddKey(string key, TranslatedString value)
        {
            var language = TranslationSystem.Instance.translationSystemSettings.translations[TranslationSystem.Instance.translationSystemSettings.translations.Length - 1];
            language.translationKeys.Add(key);
            foreach (var lang in language.languages)
            {
                lang.strings.Add(value[lang.supportedLanguage] ?? value[SupportedLanguage.English]);
            }

            TranslationSystem.Instance.IncreaseGenerationId();
        }

        private static void RemoveKey(string key)
        {
            var language = TranslationSystem.Instance.translationSystemSettings.translations[TranslationSystem.Instance.translationSystemSettings.translations.Length - 1];
            language.RemoveTranslationKey(key);
        }
    }
}
