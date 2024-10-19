using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SpinCore.Translation
{
    /// <summary>
    /// A helper class used to manually add translated strings to the game's translation pools.
    /// </summary>
    public static class TranslationHelper
    {
        private static bool _readyToAdd;
        private static void MarkReadyToAdd() => _readyToAdd = true;

        private static readonly Dictionary<string, TranslatedString> PendingTranslations = new Dictionary<string, TranslatedString>();

        /// <summary>
        /// Loads translation from a stream. Needs to be valid translation json.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        public static void LoadTranslationsFromStream(Stream stream)
        {
            string fullText;
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

        /// <summary>
        /// Manually add a translated string for the given key.
        /// </summary>
        /// <param name="key">The translation key</param>
        /// <param name="value">The translated string</param>
        public static void AddTranslation(string key, TranslatedString value)
        {
            if (!_readyToAdd)
            {
                PendingTranslations.Add(key, value);
                return;
            }

            AddKey(key, value);
        }

        /// <summary>
        /// Manually remove a translated string for the given key.
        /// </summary>
        /// <param name="key">The translation key</param>
        public static void RemoveTranslation(string key)
        {
            if (!_readyToAdd)
            {
                PendingTranslations.Remove(key);
                return;
            }

            RemoveKey(key);
        }

        internal static void AddAllPendingKeys()
        {
            MarkReadyToAdd();
            foreach (var pair in PendingTranslations)
            {
                AddKey(pair.Key, pair.Value);
            }
            PendingTranslations.Clear();
        }
        
        private static void AddKey(string key, TranslatedString value)
        {
            var language = TranslationSystem.Settings.translations[TranslationSystem.Settings.translations.Length - 1];
            language.translationKeys.Add(key);
            foreach (var lang in language.languages)
            {
                lang.strings.Add(value[lang.supportedLanguage] ?? value[SupportedLanguage.English]);
            }

            TranslationSystem.Instance.IncreaseGenerationId();
        }

        private static void RemoveKey(string key)
        {
            var language = TranslationSystem.Settings.translations[TranslationSystem.Settings.translations.Length - 1];
            language.RemoveTranslationKey(key);
        }
    }
}
