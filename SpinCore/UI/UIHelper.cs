using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpinCore.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XDMenuPlay;

namespace SpinCore.UI
{
    public static class UIHelper
    {
        public class UIPrefabs
        {
            public GameObject LargeButton { get; internal set; }
            public GameObject SidePanel { get; internal set; }
            public GameObject LargeMultiChoice { get; internal set; }
            public GameObject SmallMultiChoice { get; internal set; }
            public GameObject SettingsPage { get; internal set; }
            public GameObject Line { get; internal set; }
            public GameObject SectionHeader { get; internal set; }
            public GameObject EmptySection { get; internal set; }
            public GameObject PopoutButton { get; internal set; }
            public GameObject Image { get; internal set; }
            public GameObject Label { get; internal set; }
            public GameObject InputField { get; internal set; }
            
            internal UIPrefabs() {}
        }

        #region Fields and Properties
        public static UIPrefabs Prefabs { get; } = new UIPrefabs();

        private const string PanelNamePrefix = "TabPanel_SpinCore";
        private static XDTabPanelGroup _tabPanelGroupInstance;

        private static readonly List<CustomSidePanel> SidePanels = new List<CustomSidePanel>();
        private static readonly List<CustomSidePanel> SidePanelBuffer = new List<CustomSidePanel>();
        private static readonly List<CustomPage> PageStack = new List<CustomPage>();
        private static readonly List<(TranslationReference, CustomPage)> ModSettingsPopoutBuffer = new List<(TranslationReference, CustomPage)>();
        private static readonly List<CustomPage> CustomPageBuffer = new List<CustomPage>();
        private static CustomSidePanel _quickSettingsPanelRef;
        private static CustomPage _modSettingsPageRef;
        private static CustomActiveComponent _modSettingsListRef;
        private static CustomPage LastPageOnStack => PageStack.Count > 0 ? PageStack[PageStack.Count - 1] : null;
        internal static Transform CommonTabParent { get; set; }
        #endregion

        #region Internal Side Panel Management
        internal static void CreateQuickSettingsSidePanel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("SpinCore.Resources.Wrench.png");
            var tex = RuntimeAssetLoader.Texture2DFromStream(stream);
            const int squareBorderOffset = 50;
            var icon = Sprite.Create(tex, new Rect(squareBorderOffset, squareBorderOffset, tex.width - squareBorderOffset * 2, tex.height - squareBorderOffset * 2), Vector2.zero);
            _quickSettingsPanelRef = CreateSidePanel("SpinCoreQuickSettings", "SpinCore_QuickModSettings", icon);
        }

        internal static void LoadSidePanels(XDTabPanelGroup instance)
        {
            _tabPanelGroupInstance = instance;
            foreach (var panel in SidePanels)
                CreateSidePanelObject(panel);
        }

        internal static void CreateSidePanelObject(CustomSidePanel panel)
        {
            var panelObj = GameObject.Instantiate(Prefabs.SidePanel, CustomPrefabStore.RootTransform);
            panelObj.name = PanelNamePrefix + panel.PanelName;
            var tabConfig = new XDTabPanelGroup.TabConfig
            {
                buildFlags = new BuildCondition
                {
                    disableIfPresent = 0,
                    required = BuildCondition.BuildFlags.Steam,
                },
                name = panel.PanelName,
                prefabs = new [] {panelObj.GetComponentInChildren<XDSelectionListItemDisplay>()},
                translation = panel.Translation,
                icon = panel.Sprite,
            };
            XDTabPanelGroup.TabInstance tabInstance = new XDTabPanelGroup.TabInstance
            {
                config = tabConfig,
                translation = panel.Translation,
            };
            tabInstance.selectorButton = _tabPanelGroupInstance.CreateTabButton(tabInstance);
            tabInstance.selectorButton.transform.SetSiblingIndex(_tabPanelGroupInstance._tabInstances.Count + 1);
            _tabPanelGroupInstance._tabInstances.Add(tabInstance);
            _tabPanelGroupInstance._tabInstancesByName.Add(panel.PanelName, tabInstance);
            _tabPanelGroupInstance.UpdateTabInternal();
        }

        internal static void CallSidePanelEvent(string panelName)
        {
            if (_tabPanelGroupInstance == null) return; // This should never happen but just in case
            CustomSidePanel sidePanel;
            sidePanel = SidePanels.Find(panel => panel.PanelName == panelName);
            if (sidePanel == null)
                return;

            // FIXME: This bit of code is here to prevent a NullReferenceException due to some weird shenanigans.
            // TODO: Fix above
            // Solution: defer UI initialization by one frame
            if (!sidePanel.Loaded)
            {
                SidePanelBuffer.Add(sidePanel);
            }
        }

        internal static void CheckPanelCreation()
        {
            if (SidePanelBuffer.Count == 0)
                return;
            foreach (var sidePanel in SidePanelBuffer)
            {
                _tabPanelGroupInstance.UpdateTabInternal();
                sidePanel.PanelTransform = _tabPanelGroupInstance.tabPanelDisplay.transform.Find(PanelNamePrefix + sidePanel.PanelName + "(Clone)");
                sidePanel.PanelContentTransform = sidePanel.PanelTransform.Find("Scroll List Tab Prefab/Scroll View/Viewport/Content");
                sidePanel.OnSidePanelFocus();
            }

            SidePanelBuffer.Clear();
        }
        #endregion

        #region Internal CustomPage Management
        internal static void PushPageOnStack(CustomPage page)
        {
            if (PageStack.Contains(page)) return;
            if (LastPageOnStack != null)
                LastPageOnStack.Active = false;
            page.Active = true;
            page.OnFocus();
            PageStack.Add(page);
        }

        internal static void ClearStack()
        {
            if (LastPageOnStack != null)
                LastPageOnStack.Active = false;
            PageStack.Clear();
        }

        internal static void RemoveLastFromStack()
        {
            if (LastPageOnStack != null)
            {
                PageStack[PageStack.Count - 1].Active = false;
                PageStack.RemoveAt(PageStack.Count - 1);
                if (LastPageOnStack != null)
                    LastPageOnStack.Active = true;
            }
        }

        internal static bool AnyPagesLeftOnStack() => PageStack.Count > 0;
        
        internal static CustomPage InitializeCustomModPage()
        {
            _modSettingsPageRef = CreateSettingsPage("CustomiseModSettings");
            _modSettingsPageRef.OnPageLoad += transform =>
            {
                var modListSection = CreateGroup(
                    transform,
                    "Mod List"
                );
                CreateSectionHeader(
                    modListSection.Transform,
                    "Mod List Section Header",
                    "SpinCore_ModSettings_ModList",
                    false
                );
                foreach ((TranslationReference translation, CustomPage page) in ModSettingsPopoutBuffer)
                {
                    CreatePopout(modListSection.Transform, page.PageName + " Popout", translation, page);
                }
                ModSettingsPopoutBuffer.Clear();
                _modSettingsListRef = modListSection;
            };
            return _modSettingsPageRef;
        }

        internal static void CreateCustomPagesInBuffer()
        {
            foreach (var page in CustomPageBuffer)
            {
                CreateCustomPage(page);
            }
            CustomPageBuffer.Clear();
        }

        internal static void CreateCustomPage(CustomPage page)
        {
            page.GameObject = GameObject.Instantiate(Prefabs.SettingsPage, CommonTabParent);
            page.GameObject.name = "SpinCoreCustomTab_" + page.PageName;
            page.PageTransform = page.GameObject.transform;
            page.PageContentTransform = page.PageTransform.Find("Scroll View/Viewport/Content");
        }
        #endregion

        #region UI Component Creation
        public static void RegisterMenuInModSettingsRoot(string translationKey, CustomPage page) => RegisterMenuInModSettingsRoot(new TranslationReference(translationKey, false), page);
        public static void RegisterMenuInModSettingsRoot(TranslationReference translation, CustomPage page)
        {
            if (_modSettingsListRef != null)
            {
                CreatePopout(_modSettingsListRef.Transform, page.PageName + " Popout", translation, page);
                return;
            }
            ModSettingsPopoutBuffer.Add((translation, page));
        }

        public static void RegisterGroupInQuickModSettings(CustomSidePanel.SidePanelLoad onLoad)
        {
            if (_quickSettingsPanelRef == null)
                CreateQuickSettingsSidePanel();
            _quickSettingsPanelRef.OnSidePanelLoaded += onLoad;
        }

        public static CustomInputField CreateInputField(Transform parent, string name, Action<string, string> onValueChanged)
        {
            var textField = GameObject.Instantiate(Prefabs.InputField, parent);
            textField.name = name;
            textField.GetComponent<XDNavigableInputField>().OnValueChanged += onValueChanged;
            return new CustomInputField(textField, onValueChanged);
        }

        public static CustomImage CreateImage(Transform parent, string name, Texture tex)
        {
            var image = GameObject.Instantiate(Prefabs.Image, parent);
            image.name = name;
            image.GetComponentInChildren<SlantedRectangle>().texture = tex;
            return new CustomImage(image);
        }

        public static CustomTextComponent CreateLabel(Transform parent, string name, string translationKey) => CreateLabel(parent, name, new TranslationReference(translationKey, false));

        public static CustomTextComponent CreateLabel(Transform parent, string name, TranslationReference translation)
        {
            var text = GameObject.Instantiate(Prefabs.LargeMultiChoice.transform.Find("OptionLabel").gameObject, parent);
            text.name = name;
            text.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            return new CustomTextComponent(text);
        }

        public static CustomPage CreateSettingsPage(string name)
        {
            var page = new CustomPage(name);
            if (Prefabs.SettingsPage is null)
            {
                CustomPageBuffer.Add(page);
                return page;
            }
            CreateCustomPage(page);
            return page;
        }

        public static CustomGroup CreateGroup(Transform parent, string name, Axis layoutDirection = Axis.Vertical)
        {
            var section = GameObject.Instantiate(Prefabs.EmptySection, parent);
            section.name = name;
            var group = new CustomGroup(section);
            group.LayoutDirection = layoutDirection;
            return group;
        }

        public static CustomSectionHeader CreateSectionHeader(Transform parent, string name, string translationKey, bool spacer) => CreateSectionHeader(parent, name, new TranslationReference(translationKey, false), spacer);

        public static CustomSectionHeader CreateSectionHeader(Transform parent, string name, TranslationReference translation, bool spacer)
        {
            var obj = GameObject.Instantiate(Prefabs.SectionHeader, parent);
            obj.name = name;
            return new CustomSectionHeader(obj)
            {
                TextTranslation = translation,
                Spacer = spacer
            };
        }

        public static CustomActiveComponent CreateLine(Transform parent, string name = "Line")
        {
            var line = GameObject.Instantiate(Prefabs.Line, parent);
            line.name = name;
            return new CustomActiveComponent(line);
        }

        public static CustomPopoutButton CreatePopout(Transform parent, string name, string translationKey, CustomPage page) => CreatePopout(parent, name, new TranslationReference(translationKey, false), page);

        public static CustomPopoutButton CreatePopout(Transform parent, string name, TranslationReference translation, CustomPage page)
        {
            var button = GameObject.Instantiate(Prefabs.PopoutButton, parent);
            button.name = name;
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            button.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            button.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                PushPageOnStack(page);
            });
            return new CustomPopoutButton(button, page);
        }

        public static CustomSidePanel CreateSidePanel(string name, string translationKey, Sprite sprite = null) => CreateSidePanel(name, new TranslationReference(translationKey, false), sprite);
        public static CustomSidePanel CreateSidePanel(string name, TranslationReference translation, Sprite sprite = null)
        {
            var panel = new CustomSidePanel(name, translation, sprite);
            if (_tabPanelGroupInstance != null)
            {
                CreateSidePanelObject(panel);
            }
            SidePanels.Add(panel);
            return panel;
        }

        public static CustomButton CreateButton(Transform parent, string name, string translationKey, UnityAction action) => CreateButton(parent, name, new TranslationReference(translationKey, false), action);
        public static CustomButton CreateButton(Transform parent, string name, TranslationReference translation, UnityAction action)
        {
            var button = GameObject.Instantiate(Prefabs.LargeButton, parent);
            button.name = name;
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            button.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            button.GetComponent<XDNavigableButton>().onClick.AddListener(action);
            return new CustomButton(button);
        }

        public static CustomMultiChoice CreateSmallToggle(
            Transform parent,
            string name,
            string translationKey,
            bool defaultValue,
            Action<bool> onValueChange)
        {
            return CreateSmallToggle(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                onValueChange
            );
        }
        
        public static CustomMultiChoice CreateSmallToggle(
            Transform parent,
            string name,
            TranslationReference translation,
            bool defaultValue,
            Action<bool> onValueChange)
        {
            return CreateSmallMultiChoiceButton(
                parent,
                name,
                translation,
                defaultValue ? 1 : 0,
                v => onValueChange.Invoke(v == 1),
                () => new IntRange(0, 2),
                v => v == 1 ? "UI_Yes" : "UI_No"
            );
        }

        public static CustomMultiChoice CreateLargeToggle(
            Transform parent,
            string name,
            string translationKey,
            bool defaultValue,
            Action<bool> onValueChange)
        {
            return CreateLargeToggle(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                onValueChange
            );
        }
        
        public static CustomMultiChoice CreateLargeToggle(
            Transform parent,
            string name,
            TranslationReference translation,
            bool defaultValue,
            Action<bool> onValueChange)
        {
            return CreateLargeMultiChoiceButton(
                parent,
                name,
                translation,
                defaultValue ? 1 : 0,
                v => onValueChange.Invoke(v == 1),
                () => new IntRange(0, 2),
                v => v == 1 ? "UI_Yes" : "UI_No"
            );
        }

        public static CustomMultiChoice CreateSmallMultiChoiceButton<T>(
            Transform parent,
            string name,
            string translationKey,
            T defaultValue,
            Action<T> onValueChange) where T : Enum
        {
            return CreateSmallMultiChoiceButton(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                onValueChange
            );
        }
        
        public static CustomMultiChoice CreateSmallMultiChoiceButton<T>(
            Transform parent,
            string name,
            TranslationReference translation,
            T defaultValue,
            Action<T> onValueChange) where T : Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));
            int defaultValueInt = values.ToList().IndexOf(defaultValue);
            return CreateSmallMultiChoiceButton(
                parent,
                name,
                translation,
                defaultValueInt,
                v => onValueChange.Invoke(values[v]),
                () => new IntRange(0, values.Length),
                v => values[v].ToString()
            );
        }

        public static CustomMultiChoice CreateSmallMultiChoiceButton(
            Transform parent,
            string name,
            string translationKey,
            int defaultValue,
            XDNavigableOptionMultiChoice.OnValueChanged valueChanged,
            XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested,
            XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested)
        {
            return CreateSmallMultiChoiceButton(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                valueChanged,
                valueRangeRequested,
                valueTextRequested
            );
        }
        
        public static CustomMultiChoice CreateSmallMultiChoiceButton(
            Transform parent,
            string name,
            TranslationReference translation,
            int defaultValue,
            XDNavigableOptionMultiChoice.OnValueChanged valueChanged,
            XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested,
            XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested)
        {
            return CreateMultiChoiceButton(
                Prefabs.SmallMultiChoice,
                parent,
                name,
                translation,
                defaultValue,
                valueChanged,
                valueRangeRequested,
                valueTextRequested
            );
        }
        
        public static CustomMultiChoice CreateLargeMultiChoiceButton<T>(
            Transform parent,
            string name,
            string translationKey,
            T defaultValue,
            Action<T> onValueChange) where T : Enum
        {
            return CreateLargeMultiChoiceButton(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                onValueChange
            );
        }
        
        public static CustomMultiChoice CreateLargeMultiChoiceButton<T>(
            Transform parent,
            string name,
            TranslationReference translation,
            T defaultValue,
            Action<T> onValueChange) where T : Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));
            int defaultValueInt = values.ToList().IndexOf(defaultValue);
            return CreateLargeMultiChoiceButton(
                parent,
                name,
                translation,
                defaultValueInt,
                v => onValueChange.Invoke(values[v]),
                () => new IntRange(0, values.Length),
                v => values[v].ToString()
            );
        }

        public static CustomMultiChoice CreateLargeMultiChoiceButton(
            Transform parent,
            string name,
            string translationKey,
            int defaultValue,
            XDNavigableOptionMultiChoice.OnValueChanged valueChanged,
            XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested,
            XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested)
        {
            return CreateLargeMultiChoiceButton(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                valueChanged,
                valueRangeRequested,
                valueTextRequested
            );
        }
        
        public static CustomMultiChoice CreateLargeMultiChoiceButton(
            Transform parent,
            string name,
            TranslationReference translation,
            int defaultValue,
            XDNavigableOptionMultiChoice.OnValueChanged valueChanged,
            XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested,
            XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested)
        {
            return CreateMultiChoiceButton(
                Prefabs.LargeMultiChoice,
                parent,
                name,
                translation,
                defaultValue,
                valueChanged,
                valueRangeRequested,
                valueTextRequested
            );
        }

        private static CustomMultiChoice CreateMultiChoiceButton(
            GameObject multiChoicePrefab,
            Transform parent,
            string name,
            TranslationReference translation,
            int defaultValue,
            XDNavigableOptionMultiChoice.OnValueChanged valueChanged,
            XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested,
            XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested)
        {
            var button = GameObject.Instantiate(multiChoicePrefab, parent);
            button.name = name;
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            button.GetComponent<XDNavigableOptionMultiChoice>().state.callbacks = new XDNavigableOptionMultiChoice.Callbacks();
            button.GetComponent<XDNavigableOptionMultiChoice>().SetCallbacksAndValue(defaultValue, valueChanged, valueRangeRequested, valueTextRequested);
            button.GetComponent<XDNavigableOptionMultiChoice>().allowBarDisplay = true;
            return new CustomMultiChoice(button);
        }
        #endregion
    }
}
