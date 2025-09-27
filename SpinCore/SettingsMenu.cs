using System.IO;
using System.Reflection;
using SpinCore.UI;

namespace SpinCore
{
    internal static class SettingsMenu
    {
        private static readonly string LanguageTemplatePath =
            Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "language.json");
        
        public static void Initialize()
        {
            var page = UIHelper.CreateCustomPage("SpinCoreSettings");
            page.OnPageLoad += pageTransform =>
            {
                var languageGroup = UIHelper.CreateGroup(pageTransform, "CustomLanguages");
                UIHelper.CreateSectionHeader(languageGroup, "Header", "SpinCore_CustomLanguages", false);
                UIHelper.CreateButton(languageGroup, "DumpKeys", "SpinCore_CustomLanguage_CreateTemplate",
                    TriggerCreateTemplate);
            };
            UIHelper.RegisterMenuInModSettingsRoot("SpinCore_Name", page);
        }

        private static void TriggerCreateTemplate()
        {
            if (File.Exists(LanguageTemplatePath))
            {
                var msg = ModalMessageDialogExtensions.CreateYesNo();
                msg.message = new TranslationReference("SpinCore_CustomLanguage_ConfirmTemplateOverride", false).Translation;
                msg.affirmativeCallback = CreateTemplate;
                msg.cancelCallback = () => { };
                msg.Open();
            }
            else
                CreateTemplate();
        }

        private static void CreateTemplate()
        {
            var keys = TranslationSystem.Instance.translationKeys;
            var language = TranslationSystem.Settings.translations[TranslationSystem.Settings.translations.Length - 1]
                .GetLanguage(TranslationSystem.CurrentLanguage);
            if (File.Exists(LanguageTemplatePath))
                File.Delete(LanguageTemplatePath);
            var file = File.CreateText(LanguageTemplatePath);
            file.WriteLine("{");
            file.WriteLine("  \"id\": \"Replace this with a short identifier for your language (ex: fr, en, invert, wawa)\",");
            file.WriteLine("  \"name\": \"The name of your language as it shows up in the game\",");
            file.WriteLine("  \"keys\": {");
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                string str = language.GetString(i);
                file.Write($"    \"{key}\": \"{str.Replace("\"", "\\\"")}\"");
                if (i < keys.Count - 1)
                    file.Write(",");
                file.WriteLine();
            }
            file.WriteLine("  }");
            file.WriteLine("}");
            file.Close();
            NotificationSystemGUI.AddMessage("language.json file generated. Check your BepInEx/plugins folder");
        }
    }
}
