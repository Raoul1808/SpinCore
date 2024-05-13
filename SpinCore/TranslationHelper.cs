using System.Collections.Generic;

namespace SpinCore
{
    public static class TranslationHelper
    {
        private static bool _readyToAdd = false;
        private static void MarkReadyToAdd() => _readyToAdd = true;

        private static Dictionary<string, string> _pendingTranslations = new Dictionary<string, string>();

        public static void AddTranslationKey(string key, string value)
        {
            if (!_readyToAdd)
            {
                _pendingTranslations.Add(key, value);
                return;
            }

            AddKey(key, value);
        }

        public static void RemoveTranslationKey(string key)
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
        
        private static void AddKey(string key, string value)
        {
            var language = TranslationSystem.Instance.translationSystemSettings.translations[TranslationSystem.Instance.translationSystemSettings.translations.Length - 1];
            language.translationKeys.Add(key);
            foreach (var lang in language.languages)
            {
                lang.strings.Add(value);
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
