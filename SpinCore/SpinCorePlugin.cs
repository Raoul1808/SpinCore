using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpinCore.Patches;
using SpinCore.Translation;

namespace SpinCore
{
    /// <summary>
    /// The BepInEx plugin for SpinCore.
    /// </summary>
    [BepInPlugin(Guid, Name, Version)]
    public class SpinCorePlugin : BaseUnityPlugin
    {
        /// <summary>
        /// SpinCore's plugin Guid. This should be used for dependencies.
        /// </summary>
        public const string Guid = "srxd.raoul1808.spincore";

        /// <summary>
        /// SpinCore's plugin name. This is used exclusively for display and logging purposes.
        /// </summary>
        public const string Name = "SpinCore";

        /// <summary>
        /// SpinCore's plugin version. This should be used for version-specific dependencies.
        /// </summary>
        public const string Version = "1.2.0";

        private static ManualLogSource _logger;
        
        private void Awake()
        {
            _logger = Logger;
            Logger.LogMessage($"Hello from {Name}");
            Harmony harmony = new Harmony(Guid);
            harmony.PatchAll(typeof(TranslationPatches));
            harmony.PatchAll(typeof(UIPatches));
            harmony.PatchAll(typeof(TriggerPatches));

            var localeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpinCore.locale.json");
            TranslationHelper.LoadTranslationsFromStream(localeStream);
            LoadLanguages();
        }

        private void LoadLanguages()
        {
            var langPath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
                "CustomLanguages");
            foreach (var filepath in Directory.EnumerateFiles(langPath))
            {
                FileStream file = null;
                try
                {
                    file = File.OpenRead(filepath);
                    LanguageHelper.LoadCustomLanguageFromStream(file);
                }
                catch (Exception e)
                {
                    LogInfo($"Failed to load language at {filepath}: {e}");
                }
                finally
                {
                    file?.Close();
                }
            }
        }
        
        internal static void LogInfo(object msg) => _logger.LogInfo(msg);
    }
}
