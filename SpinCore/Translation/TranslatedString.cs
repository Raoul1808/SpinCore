using JetBrains.Annotations;

namespace SpinCore.Translation
{
    /// <summary>
    /// A container for a translated string in all languages supported by the game.
    /// </summary>
    public struct TranslatedString
    {
        [NotNull]
        public string en;
        public string fr;
        public string de;
        public string es;
        public string it;
        public string zh_cn;
        public string zh_tw;
        public string jp;
        public string kr;
        public string id;
        public string pt;
        public string tr;
        public string ru;
        public string pl;
        public string nl;

        public static implicit operator TranslatedString(string value) => new TranslatedString { en = value };

        public string this[SupportedLanguage language]
        {
            get
            {
                switch (language)
                {
                    case SupportedLanguage.English:
                        return en;
                    case SupportedLanguage.French:
                        return fr;
                    case SupportedLanguage.Italian:
                        return it;
                    case SupportedLanguage.German:
                        return de;
                    case SupportedLanguage.Spanish:
                        return es;
                    case SupportedLanguage.ChineseSimplified:
                        return zh_cn;
                    case SupportedLanguage.ChineseTraditional:
                        return zh_tw;
                    case SupportedLanguage.Japanese:
                        return jp;
                    case SupportedLanguage.Korean:
                        return kr;
                    case SupportedLanguage.Indonesian:
                        return id;
                    case SupportedLanguage.PortugueseBrazil:
                        return pt;
                    case SupportedLanguage.Turkish:
                        return tr;
                    case SupportedLanguage.Russian:
                        return ru;
                    case SupportedLanguage.Polish:
                        return pl;
                    case SupportedLanguage.Dutch:
                        return nl;
                    default:
                        return null;
                }
            }
        }
    }
}
