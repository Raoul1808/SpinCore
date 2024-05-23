using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpinCore.Patches;
using SpinCore.Translation;

namespace SpinCore
{
    [BepInPlugin(Guid, Name, Version)]
    internal class Plugin : BaseUnityPlugin
    {
        private const string Guid = "srxd.raoul1808.spincore";
        private const string Name = "SpinCore";
        private const string Version = "0.1.0";

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
        }

        internal static void LogInfo(object msg) => _logger.LogInfo(msg);
    }
}
