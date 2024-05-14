using System.Diagnostics;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpinCore.Patches;

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
            
            TranslationHelper.AddTranslationKey("SpinCore_ModTab", "Quick Mod Settings");
            TranslationHelper.AddTranslationKey("SpinCore_HelloWorld", "Hello World!");
            TranslationHelper.AddTranslationKey("SpinCore_SecondTestButton", "Free banger on YouTube");

            UIHelper.OnSidePanelLoaded += parent =>
            {
                UIHelper.CreateButton(
                    parent,
                    "HelloWorld",
                    new TranslationReference("SpinCore_HelloWorld", false),
                    () => { NotificationSystemGUI.AddMessage("Hello, world!"); }
                );
                UIHelper.CreateButton(
                    parent,
                    "FreeBanger",
                    new TranslationReference("SpinCore_SecondTestButton", false),
                    () => { Process.Start("https://www.youtube.com/watch?v=RZHR4DtWufo"); }
                );
            };
        }

        internal static void LogInfo(object msg) => _logger.LogInfo(msg);
    }
}
