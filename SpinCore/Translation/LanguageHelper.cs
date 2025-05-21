using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SpinCore.Translation
{
    /// <summary>
    /// A helper class used to add custom languages in the game.
    /// </summary>
    public static class LanguageHelper
    {
        private const int BaseLanguageCount = (int)SupportedLanguage.Count;
        private static bool _languagesLoaded = false;
        private static int _customLanguages = 0;
        internal static int LanguageCount => BaseLanguageCount + _customLanguages;
        private static readonly List<CustomLanguage> LanguageBuffer = new List<CustomLanguage>();

        public static void LoadCustomLanguageFromStream(Stream stream)
        {
            if (_languagesLoaded)
                throw new Exception("Cannot load new languages after game startup");
            string fullText;
            using (StreamReader reader = new StreamReader(stream))
            {
                fullText = reader.ReadToEnd();
            }

            var language = JsonConvert.DeserializeObject<CustomLanguage>(fullText, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                },
            });
            LanguageBuffer.Add(language);
        }

        public static void AddLanguage(CustomLanguage language)
        {
            if (_languagesLoaded)
                throw new Exception("Cannot load new languages after game startup");
            LanguageBuffer.Add(language);
        }
        internal static void AddPendingLanguages()
        {
            if (_languagesLoaded)
                return;
            _languagesLoaded = true;
            foreach (var customLanguage in LanguageBuffer)
            {
                var languageInfo = new LanguageInfo()
                {
                    appStoreLocaleID = customLanguage.Id,
                    enabledInBuild = true,
                    languageName = customLanguage.Name,
                    localeID = customLanguage.Id,
                    supportedLanguage = (SupportedLanguage)LanguageCount,
                };
                TranslationSystem.Instance.EnabledLanguages.Add(languageInfo);
                {
                    var arr = new List<LanguageInfo>(TranslationSystem.Settings.languageSettings.languageInfos)
                        { languageInfo };
                    TranslationSystem.Settings.languageSettings.languageInfos = arr.ToArray();
                }
                foreach (var language in TranslationSystem.Settings.translations)
                {
                    var lang = new Translations.Language()
                    {
                        strings = new List<string>(),
                        supportedLanguage = (SupportedLanguage)LanguageCount,
                    };

                    var english = language.GetLanguage(SupportedLanguage.English);
                    for (int i = 0; i < language.translationKeys.Count; i++)
                    {
                        string key = language.translationKeys[i];
                        string value = customLanguage.Keys.TryGetValue(key, out string str)
                            ? str
                            : english.GetString(i);
                        lang.strings.Add(value);
                    }

                    language.languages.Add(lang);
                }

                _customLanguages++;
            }
        }
    }
}
