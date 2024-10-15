using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using SpinCore.Translation;
using SpinCore.Triggers;
using SpinCore.UI;
using SpinCore.Utility;
using UnityEngine;

namespace SpinCore.TestMod
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("srxd.raoul1808.spincore", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string Guid = "srxd.raoul1808.spincore.testmod";
        private const string Name = "SpinCore Test Mod";
        private const string Version = "1.0.0";

        private static ManualLogSource _logger;

        private enum Modders
        {
            Mew,
            Aexus,
            Pink,
            Edge,
        }

        private Texture2D LoadImage(string name)
        {
            using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpinCore.TestMod.Resources." + name))
            {
                return RuntimeAssetLoader.Texture2DFromStream(imageStream);
            }
        }

        private void Awake()
        {
            // Load images embedded in the mod
            var tabIcon = LoadImage("TestModIcon.png");
            const int squareBorderOffset = 10;
            var sprite = Sprite.Create(tabIcon, new Rect(squareBorderOffset, squareBorderOffset, tabIcon.width - squareBorderOffset * 2, tabIcon.height - squareBorderOffset * 2), Vector2.zero);

            var sproing = LoadImage("sproing.png");

            _logger = Logger;
            LogInfo($"Hello from {Name}!");

            // Load locale data embedded in the mod
            var localeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpinCore.TestMod.locale.json");
            TranslationHelper.LoadTranslationsFromStream(localeStream);

            // Manual translation also works
            // An implicit operator is defined so that "This string" will translate to TranslatedString { en = "This string" }
            TranslationHelper.AddTranslation("TestKey", "TestString");
            
            // Register a new group in the quick mod settings panel
            UIHelper.RegisterGroupInQuickModSettings(parent =>
            {
                UIHelper.CreateLabel(parent, "Test", "SpinCore_TestMod_HelloWorld");
            });

            // Create a new custom page to be used in the mod settings page
            var testSettings = UIHelper.CreateCustomPage("TestPopout");
            
            // Custom pages are lazily loaded, so a callback must be defined
            testSettings.OnPageLoad += pageTransform =>
            {
                // The UI definition should be self-explanatory
                var section = UIHelper.CreateGroup(pageTransform, "Test Section");
                UIHelper.CreateSectionHeader(
                    section.Transform,
                    "Section Header",
                    "SpinCore_TestMod_ModSettings_TestPopoutHeader",
                    false
                );
                UIHelper.CreateButton(
                    section.Transform,
                    "Test Button",
                    "SpinCore_TestMod_ModSettings_TestPopoutButton",
                    () => NotificationSystemGUI.AddMessage("Test Button clicked!")
                );
                {
                    var subSection = UIHelper.CreateGroup(section.Transform, "Test Image Container");
                    subSection.LayoutDirection = Axis.Horizontal;
                    UIHelper.CreateImage(
                        subSection.Transform,
                        "Test Image",
                        sproing
                    );
                    UIHelper.CreateLabel(
                        subSection.Transform,
                        "Test Label",
                        "SpinCore_TestMod_ModSettings_TestLabel"
                    );
                    // subSection.GameObject.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(230, 10, 10, 10);
                }
                UIHelper.CreateInputField(
                    section.Transform,
                    "Test Input Field",
                    (oldVal, newVal) => Plugin.LogInfo("Looking for " + oldVal + " of " + newVal)
                );
                UIHelper.CreateLargeToggle(
                    section.Transform,
                    "Test Large Toggle",
                    "SpinCore_TestMod_ModSettings_TestLargeToggle",
                    false,
                    (val) => NotificationSystemGUI.AddMessage(val ? "Hi" : "Bye")
                );
            };
            
            // Once the custom page is created, you can add it to the mod settings menu with this method.
            UIHelper.RegisterMenuInModSettingsRoot("SpinCore_TestMod_ModSettings_TestPopout", testSettings);

            // Next, let's create a custom side panel
            var modPanel = UIHelper.CreateSidePanel("QuickModSettings", "SpinCore_TestMod_ModTab", sprite);

            // Side panels are also lazily loaded
            modPanel.OnSidePanelLoaded += parent =>
            {
                // This should once again be self-explanatory
                int value = 0;
                UIHelper.CreateButton(
                    parent,
                    "HelloWorld",
                    "SpinCore_TestMod_HelloWorld",
                    () => { NotificationSystemGUI.AddMessage("Hello, world!"); }
                );
                var notifyButton = UIHelper.CreateButton(
                    parent,
                    "ShowValue",
                    "SpinCore_TestMod_SecondTestButton",
                    () => { NotificationSystemGUI.AddMessage("Value: " + value); }
                );
                UIHelper.CreateSmallMultiChoiceButton(
                    parent,
                    "ShiftValue",
                    "SpinCore_TestMod_ShiftValue",
                    0,
                    v =>
                    {
                        value = v;
                        notifyButton.ExtraText = v.ToString();
                    },
                    () => new IntRange(0, 101),
                    v => v.ToString()
                );
                UIHelper.CreateSmallMultiChoiceButton(
                    parent,
                    "BestModder",
                    "SpinCore_TestMod_BestModder",
                    Modders.Mew,
                    modder => NotificationSystemGUI.AddMessage("The new best modder is " + modder)
                );
                UIHelper.CreateSmallToggle(
                    parent,
                    "TestToggle",
                    "SpinCore_TestMod_TestToggle",
                    false,
                    enable => NotificationSystemGUI.AddMessage("Enabled: " + enable)
                );

                // Here we want to make a subsection with a horizontal layout
                var section = UIHelper.CreateGroup(parent, "Test Section", Axis.Horizontal);

                // To target the subsection, simply change the target transform
                UIHelper.CreateButton(
                    section.Transform,
                    "TestHorizontalButton1",
                    "SpinCore_TestMod_FirstButton",
                    () => NotificationSystemGUI.AddMessage("Am First Button bloop bloop")
                );
                UIHelper.CreateButton(
                    section.Transform,
                    "TestHorizontalButton2",
                    "SpinCore_TestMod_SecondButton",
                    () => NotificationSystemGUI.AddMessage("Button, The 2nd")
                );
                UIHelper.CreateButton(
                    section.Transform,
                    "TestHorizontalButton3",
                    "SpinCore_TestMod_ThirdButton",
                    () => NotificationSystemGUI.AddMessage("third button's the charm")
                );
                
                UIHelper.CreateInputField(
                    parent,
                    "Test Input Field From Side Bar",
                    (s, newVal) => { if (!string.IsNullOrWhiteSpace(newVal)) NotificationSystemGUI.AddMessage("Hello " + newVal + "!"); }
                );

                UIHelper.CreateButton(
                    parent,
                    "Test Custom Dialog Component",
                    "SpinCore_TestMod_TestCustomDialog",
                    () =>
                    {
                        // Here is a demonstration of how to add custom UI components to the modal message dialog
                        var msg = ModalMessageDialogExtensions.CreateYesNo();
                        msg.message = "Hello!";
                        string buffer = "";
                        msg.cancelCallback += () =>
                        {
                            NotificationSystemGUI.AddMessage(":(");
                        };
                        msg.affirmativeCallback += () =>
                        {
                            NotificationSystemGUI.AddMessage("Hi " + buffer + "!");
                        };
                        msg.OpenWithCustomUI(modalParent =>
                        {
                            UIHelper.CreateInputField(
                                modalParent,
                                "Modal Input",
                                (s, newVal) => buffer = newVal
                            );
                        });
                    }
                );
            };

            // Trigger management
            // Here we load 4 triggers when loading any track
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
                
                // The trigger manager by default takes the trigger's class name and the mod's assembly name to define a key.
                // You can manually specify a key, but the assembly name will still be added to it.
                TriggerManager.LoadTriggers(triggers);
            };

            string lastMessage = "";
            
            // Here we register a function to be called every time a trigger is encountered and gone past.
            // Once again, the trigger's class name and the assembly are taken for the key.
            // The key must match the triggers loaded, otherwise the trigger event simply won't be called.
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
