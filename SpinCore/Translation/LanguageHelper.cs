using System.Collections.Generic;
using UnityEngine;

namespace SpinCore.Translation
{
    public static class LanguageHelper
    {
        private static bool _added = false;

        internal static void AddNewLanguage()
        {
            if (_added) return;
            _added = true;
            var languageInfo = new LanguageInfo()
            {
                appStoreLocaleID = "wawa",
                enabledInBuild = true,
                languageName = "the wawa langyuage",
                localeID = "wawa",
                supportedLanguage = (SupportedLanguage)15,
            };
            TranslationSystem.Instance.EnabledLanguages.Add(languageInfo);
            {
                var arr = new List<LanguageInfo>(TranslationSystem.Settings.languageSettings.languageInfos) { languageInfo };
                TranslationSystem.Settings.languageSettings.languageInfos = arr.ToArray();
            }
            foreach (var language in TranslationSystem.Settings.translations)
            {
                var lang = new Translations.Language()
                {
                    strings = new List<string>(),
                    supportedLanguage = (SupportedLanguage)15,
                };

                var english = language.GetLanguage(SupportedLanguage.English);
                for (int i = 0; i < language.translationKeys.Count; i++)
                {
                    lang.strings.Add(Random.Range(0, 2) % 2 == 0 ? "wawa" : english.GetString(i));
                }

                language.languages.Add(lang);
            }
        }

        internal static int LanguageCount => 16;
    }
}
