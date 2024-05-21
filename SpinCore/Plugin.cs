using System.Diagnostics;
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
            harmony.PatchAll(typeof(TriggerPatches));
            
            TranslationHelper.AddTranslationKey("SpinCore_ModTab", "Quick Mod Settings");
            TranslationHelper.AddTranslationKey("SpinCore_HelloWorld", "Hello World!");
            TranslationHelper.AddTranslationKey("SpinCore_SecondTestButton", "Notify");
            TranslationHelper.AddTranslationKey("SpinCore_ShiftValue", "Shift Value");
            TranslationHelper.AddTranslationKey("SpinCore_BestModder", "Best Modder");
            TranslationHelper.AddTranslationKey("SpinCore_TestToggle", "Test Toggle");
            TranslationHelper.AddTranslationKey("SpinCore_TanocTab", "HARDCORE TANO*C");
            TranslationHelper.AddTranslationKey("SpinCore_CustomiseModsTabButton", "Mod Settings");
            TranslationHelper.AddTranslationKey("SpinCore_ModSettings_ModList", "Mods");
            TranslationHelper.AddTranslationKey("SpinCore_ModSettings_TestPopout", "Test Popout UI");
            TranslationHelper.AddTranslationKey("SpinCore_ModSettings_TestPopoutHeader", "Test Popout Header");
            TranslationHelper.AddTranslationKey("SpinCore_ModSettings_TestPopoutButton", "Test Popout Button");

            var testSettings = UIHelper.CreateSettingsPage("TestPopout");
            testSettings.OnPageLoad += pageTransform =>
            {
                var section = UIHelper.CreateSection(pageTransform, "Test Section");
                UIHelper.CreateSectionHeader(
                    section.Transform,
                    "Section Header",
                    "SpinCore_ModSettings_TestPopoutHeader"
                );
                UIHelper.CreateButton(
                    section.Transform,
                    "Test Button",
                    "SpinCore_ModSettings_TestPopoutButton",
                    () => NotificationSystemGUI.AddMessage("Test Button clicked!")
                );
            };
            UIHelper.RegisterMenuInModSettingsRoot("SpinCore_ModSettings_TestPopout", testSettings);

            var modPanel = UIHelper.CreateSidePanel("QuickModSettings", "SpinCore_ModTab");
            modPanel.OnSidePanelLoaded += parent =>
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

            var tanocTab = UIHelper.CreateSidePanel("TanocTab", "SpinCore_TanocTab");
            tanocTab.OnSidePanelLoaded += parent =>
            {
                UIHelper.CreateButton(
                    parent,
                    "TanocButton",
                    "SpinCore_TanocTab",
                    () => { Process.Start("https://www.youtube.com/@tanoc_official"); }
                );
            };
        }

        internal static void LogInfo(object msg) => _logger.LogInfo(msg);
    }
}
