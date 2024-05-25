using System.Diagnostics;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using SpinCore.Translation;
using SpinCore.Triggers;
using SpinCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SpinCore.TestMod
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("srxd.raoul1808.spincore", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string Guid = "srxd.raoul1808.spincore.testmod";
        private const string Name = "SpinCore Test Mod";
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
            byte[] imageData;
            using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpinCore.TestMod.Resources.TestModIcon.png"))
            {
                using (MemoryStream mem = new MemoryStream())
                {
                    imageStream.CopyTo(mem);
                    imageData = mem.ToArray();
                }
            }
            var tex = new Texture2D(1, 1);
            tex.LoadImage(imageData);
            const int squareBorderOffset = 10;
            var sprite = Sprite.Create(tex, new Rect(squareBorderOffset, squareBorderOffset, tex.width - squareBorderOffset * 2, tex.height - squareBorderOffset * 2), Vector2.zero);

            _logger = Logger;
            LogInfo($"Hello from {Name}!");

            var localeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpinCore.TestMod.locale.json");
            TranslationHelper.LoadTranslationsFromStream(localeStream);

            var testSettings = UIHelper.CreateSettingsPage("TestPopout");
            testSettings.OnPageLoad += pageTransform =>
            {
                var section = UIHelper.CreateSection(pageTransform, "Test Section");
                UIHelper.CreateSectionHeader(
                    section.Transform,
                    "Section Header",
                    "SpinCore_ModSettings_TestPopoutHeader",
                    false
                );
                UIHelper.CreateButton(
                    section.Transform,
                    "Test Button",
                    "SpinCore_ModSettings_TestPopoutButton",
                    () => NotificationSystemGUI.AddMessage("Test Button clicked!")
                );
            };
            UIHelper.RegisterMenuInModSettingsRoot("SpinCore_ModSettings_TestPopout", testSettings);

            var modPanel = UIHelper.CreateSidePanel("QuickModSettings", "SpinCore_ModTab", sprite);
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

                var section = UIHelper.CreateSection(parent, "Test Section");
                DestroyImmediate(section.GameObject.GetComponent<VerticalLayoutGroup>());
                section.GameObject.AddComponent<HorizontalLayoutGroup>();

                UIHelper.CreateButton(
                    section.Transform,
                    "TestHorizontalButton1",
                    "SpinCore_FirstButton",
                    () => NotificationSystemGUI.AddMessage("Am First Button bloop bloop")
                );
                UIHelper.CreateButton(
                    section.Transform,
                    "TestHorizontalButton2",
                    "SpinCore_SecondButton",
                    () => NotificationSystemGUI.AddMessage("Button, The 2nd")
                );
                UIHelper.CreateButton(
                    section.Transform,
                    "TestHorizontalButton3",
                    "SpinCore_ThirdButton",
                    () => NotificationSystemGUI.AddMessage("third button's the charm")
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

            Track.OnLoadedIntoTrack += (handle, states) =>
            {
                var triggers = new[]
                {
                    new TestTrigger
                    {
                        Message = "Initial Trigger",
                        Time = 0f,
                    },
                    new TestTrigger
                    {
                        Message = "This should fire at 2 seconds",
                        Time = 2f,
                    },
                    new TestTrigger
                    {
                        Message = "This should fire at 5 seconds",
                        Time = 5f,
                    },
                    new TestTrigger
                    {
                        Message = "This should fire at 3.5 seconds",
                        Time = 3.5f,
                    }
                };
                TriggerManager.LoadTriggers(triggers);
            };

            string lastMessage = "";
            TriggerManager.RegisterTriggerEvent<TestTrigger>((trigger, trackTime) =>
            {
                if (trigger.Message == lastMessage) return;
                LogInfo(trigger.Message);
                lastMessage = trigger.Message;
            });
        }

        internal static void LogInfo(object msg) => _logger.LogMessage(msg);
    }
}
