using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpinCore.Patches;
using SpinCore.UI;

namespace SpinCore
{
    [BepInPlugin(Guid, Name, Version)]
    internal class Plugin : BaseUnityPlugin
    {
        private const string Guid = "srxd.raoul1808.spincore";
        private const string Name = "SpinCore";
        private const string Version = "0.1.0";

        private static ManualLogSource _logger;

        private enum Modders
        {
            Mew,
            Aexus,
            Pink,
            Edge,
        }
        
        private void Awake()
        {
            _logger = Logger;
            Logger.LogMessage($"Hello from {Name}");
            Harmony harmony = new Harmony(Guid);
            harmony.PatchAll(typeof(TranslationPatches));
            harmony.PatchAll(typeof(UIPatches));
            
            TranslationHelper.AddTranslationKey("SpinCore_ModTab", "Quick Mod Settings");
            TranslationHelper.AddTranslationKey("SpinCore_HelloWorld", "Hello World!");
            TranslationHelper.AddTranslationKey("SpinCore_SecondTestButton", "Notify");
            TranslationHelper.AddTranslationKey("SpinCore_ShiftValue", "Shift Value");
            TranslationHelper.AddTranslationKey("SpinCore_BestModder", "Best Modder");
            TranslationHelper.AddTranslationKey("SpinCore_TestToggle", "Test Toggle");

            UIHelper.OnSidePanelLoaded += parent =>
            {
                int value = 0;
                UIHelper.CreateButton(
                    parent,
                    "HelloWorld",
                    "SpinCore_HelloWorld",
                    () => { NotificationSystemGUI.AddMessage("Hello, world!"); }
                );
                var notifyButton = UIHelper.CreateButton(
                    parent,
                    "ShowValue",
                    "SpinCore_SecondTestButton",
                    () => { NotificationSystemGUI.AddMessage("Value: " + value); }
                );
                UIHelper.CreateMultiChoiceButton(
                    parent,
                    "ShiftValue",
                    "SpinCore_ShiftValue",
                    0,
                    v =>
                    {
                        value = v;
                        notifyButton.ExtraText = v.ToString();
                    },
                    () => new IntRange(0, 101),
                    v => v.ToString()
                );
                UIHelper.CreateMultiChoiceButton(
                    parent,
                    "BestModder",
                    "SpinCore_BestModder",
                    Modders.Mew,
                    modder => NotificationSystemGUI.AddMessage("The new best modder is " + modder)
                );
                UIHelper.CreateToggle(
                    parent,
                    "TestToggle",
                    "SpinCore_TestToggle",
                    false,
                    enable => NotificationSystemGUI.AddMessage("Enabled: " + enable)
                );
            };
        }

        internal static void LogInfo(object msg) => _logger.LogInfo(msg);
    }
}
