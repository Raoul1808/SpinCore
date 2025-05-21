using System.Collections.Generic;

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

                foreach (var key in language.translationKeys)
                {
                    lang.strings.Add("wawa");
                }

                language.languages.Add(lang);
            }
        }

        internal static int LanguageCount => 16;
    }
}
