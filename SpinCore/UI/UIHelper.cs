using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpinCore.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XDMenuPlay;
using Object = UnityEngine.Object;

namespace SpinCore.UI
{
    /// <summary>
    /// A helper class used to create UI components.
    /// </summary>
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
            public GameObject TooltipPopout { get; internal set; }
            
            internal UIPrefabs() {}
        }

        #region Fields and Properties
        /// <summary>
        /// Stores all cloned UI prefabs.
        /// </summary>
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
            var panelObj = Object.Instantiate(Prefabs.SidePanel, CustomPrefabStore.RootTransform);
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
            _tabPanelGroupInstance.CreateTabButton(tabInstance, false);
            _tabPanelGroupInstance.CreateTabButton(tabInstance, true);
            tabInstance.button.selectorButton.transform.SetSiblingIndex(_tabPanelGroupInstance._tabInstances.Count + 1);
            tabInstance.verticalButton.selectorButton.transform.SetSiblingIndex(_tabPanelGroupInstance._tabInstances.Count + 1);
            // tabInstance.selectorButton.transform.SetSiblingIndex(_tabPanelGroupInstance._tabInstances.Count + 1);
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
            _modSettingsPageRef = CreateCustomPage("CustomiseModSettings");
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
            page.GameObject = Object.Instantiate(Prefabs.SettingsPage, CommonTabParent);
            page.GameObject.name = "SpinCoreCustomTab_" + page.PageName;
            page.PageTransform = page.GameObject.transform;
            page.PageContentTransform = page.PageTransform.Find("Scroll View/Viewport/Content");
            page.PageTransform.Find("Scroll View").GetComponent<CustomScrollRect>().content = page.PageContentTransform.GetComponent<RectTransform>();
        }
        #endregion

        #region UI Component Creation
        /// <summary>
        /// Registers the given <see cref="CustomPage"/> to SpinCore's Mod Settings menu and creates a button to open the page.
        /// </summary>
        /// <param name="translationKey">A translation key used for the button's label</param>
        /// <param name="page">The page to register</param>
        public static void RegisterMenuInModSettingsRoot(string translationKey, CustomPage page) => RegisterMenuInModSettingsRoot(new TranslationReference(translationKey, false), page);

        /// <summary>
        /// Registers the given <see cref="CustomPage"/> to SpinCore's Mod Settings menu and creates a button to open the page.
        /// </summary>
        /// <param name="translation">A translation reference used for the button's label</param>
        /// <param name="page">The page to register</param>
        public static void RegisterMenuInModSettingsRoot(TranslationReference translation, CustomPage page)
        {
            if (_modSettingsListRef != null)
            {
                CreatePopout(_modSettingsListRef.Transform, page.PageName + " Popout", translation, page);
                return;
            }
            ModSettingsPopoutBuffer.Add((translation, page));
        }

        /// <summary>
        /// Registers the given callback to be used for when the quick mod settings tab is created.
        /// </summary>
        /// <param name="onLoad">A callback function</param>
        public static void RegisterGroupInQuickModSettings(CustomSidePanel.SidePanelLoad onLoad)
        {
            if (_quickSettingsPanelRef == null)
                CreateQuickSettingsSidePanel();
            // Adding this to silence warnings smh
            if (_quickSettingsPanelRef != null)
                _quickSettingsPanelRef.OnSidePanelLoaded += onLoad;
        }

        /// <summary>
        /// Adds a tooltip to a custom UI component. Works only in the Options menu, not in side panels.
        /// </summary>
        /// <param name="component">The component to add the tooltip to</param>
        /// <param name="translationKey">A translation key for the text to be shown</param>
        /// <returns>The tooltip added</returns>
        public static CustomTooltip AddTooltip(CustomActiveComponent component, string translationKey) => AddTooltip(component, new TranslationReference(translationKey, false));

        /// <summary>
        /// Adds a tooltip to a custom UI component. Works only in the Options menu, not in side panels.
        /// </summary>
        /// <param name="component">The component to add the tooltip to</param>
        /// <param name="translation">A translation reference for the text to be shown</param>
        /// <returns>The tooltip added</returns>
        public static CustomTooltip AddTooltip(CustomActiveComponent component, TranslationReference translation)
        {
            var tooltipOpener = component.GameObject.AddComponent<XDTooltipPopoutOpener>();
            tooltipOpener.tooltip = translation;
            tooltipOpener.tooltipPrefab = Prefabs.TooltipPopout;
            return new CustomTooltip(tooltipOpener);
        }

        /// <summary>
        /// Creates an input field UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="onValueChanged">A function called when text is changed</param>
        /// <returns>The input field component</returns>
        public static CustomInputField CreateInputField(Transform parent, string name, Action<string, string> onValueChanged)
        {
            var textField = Object.Instantiate(Prefabs.InputField, parent);
            textField.name = name;
            textField.GetComponent<XDNavigableInputField>().OnValueChanged += onValueChanged;
            return new CustomInputField(textField, onValueChanged);
        }

        /// <summary>
        /// Creates an image UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="tex">The texture to use for the image</param>
        /// <returns>The image component</returns>
        public static CustomImage CreateImage(Transform parent, string name, Texture tex)
        {
            var image = Object.Instantiate(Prefabs.Image, parent);
            image.name = name;
            image.GetComponentInChildren<SlantedRectangle>().texture = tex;
            return new CustomImage(image);
        }

        /// <summary>
        /// Creates a label UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the label</param>
        /// <returns>The label component</returns>
        public static CustomTextComponent CreateLabel(Transform parent, string name, string translationKey) => CreateLabel(parent, name, new TranslationReference(translationKey, false));

        /// <summary>
        /// Creates a label UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the label</param>
        /// <returns>The label component</returns>
        public static CustomTextComponent CreateLabel(Transform parent, string name, TranslationReference translation)
        {
            var text = Object.Instantiate(Prefabs.LargeMultiChoice.transform.Find("OptionLabel").gameObject, parent);
            text.name = name;
            text.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            return new CustomTextComponent(text);
        }

        /// <summary>
        /// Creates a custom page
        /// </summary>
        /// <param name="name">The internal name of the custom page</param>
        /// <returns>The custom page</returns>
        public static CustomPage CreateCustomPage(string name)
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

        /// <summary>
        /// Creates a custom UI group
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="layoutDirection">The group's Layout Direction</param>
        /// <returns>The custom component</returns>
        public static CustomGroup CreateGroup(Transform parent, string name, Axis layoutDirection = Axis.Vertical)
        {
            var section = Object.Instantiate(Prefabs.EmptySection, parent);
            section.name = name;
            var group = new CustomGroup(section);
            group.LayoutDirection = layoutDirection;
            return group;
        }

        /// <summary>
        /// Creates a section header UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the section header text</param>
        /// <param name="spacer">Whether to insert extra space above or not</param>
        /// <returns>The section header component</returns>
        public static CustomSectionHeader CreateSectionHeader(Transform parent, string name, string translationKey, bool spacer) => CreateSectionHeader(parent, name, new TranslationReference(translationKey, false), spacer);

        /// <summary>
        /// Creates a section header UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the section header text</param>
        /// <param name="spacer">Whether to insert extra space above or not</param>
        /// <returns>The section header component</returns>
        public static CustomSectionHeader CreateSectionHeader(Transform parent, string name, TranslationReference translation, bool spacer)
        {
            var obj = Object.Instantiate(Prefabs.SectionHeader, parent);
            obj.name = name;
            return new CustomSectionHeader(obj)
            {
                TextTranslation = translation,
                Spacer = spacer
            };
        }

        /// <summary>
        /// Creates a line separator UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <returns>The line separator component</returns>
        public static CustomActiveComponent CreateLine(Transform parent, string name = "Line")
        {
            var line = Object.Instantiate(Prefabs.Line, parent);
            line.name = name;
            return new CustomActiveComponent(line);
        }

        /// <summary>
        /// Creates a popout button UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the popout button</param>
        /// <param name="page">The page to open on click</param>
        /// <returns>The popout button component</returns>
        public static CustomPopoutButton CreatePopout(Transform parent, string name, string translationKey, CustomPage page) => CreatePopout(parent, name, new TranslationReference(translationKey, false), page);

        /// <summary>
        /// Creates a popout button UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the popout button</param>
        /// <param name="page">The page to open on click</param>
        /// <returns>The popout button component</returns>
        public static CustomPopoutButton CreatePopout(Transform parent, string name, TranslationReference translation, CustomPage page)
        {
            var button = Object.Instantiate(Prefabs.PopoutButton, parent);
            button.name = name;
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            button.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            button.GetComponent<XDNavigableButton>().onClick.AddListener(CustomButton.PlayButtonSound);
            button.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                PushPageOnStack(page);
            });
            return new CustomPopoutButton(button, page);
        }

        /// <summary>
        /// Creates a custom side panel
        /// </summary>
        /// <param name="name">The internal name of the side panel</param>
        /// <param name="translationKey">A translation key for the panel's name</param>
        /// <param name="sprite">The icon used for the side panel</param>
        /// <returns>The custom side panel</returns>
        public static CustomSidePanel CreateSidePanel(string name, string translationKey, Sprite sprite = null) => CreateSidePanel(name, new TranslationReference(translationKey, false), sprite);

        /// <summary>
        /// Creates a custom side panel
        /// </summary>
        /// <param name="name">The internal name of the side panel</param>
        /// <param name="translation">A translation reference for the panel's name</param>
        /// <param name="sprite">The icon used for the side panel</param>
        /// <returns>The custom side panel</returns>
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

        /// <summary>
        /// Creates a button UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the button</param>
        /// <param name="action">Function called on click</param>
        /// <returns>The button component</returns>
        public static CustomButton CreateButton(Transform parent, string name, string translationKey, UnityAction action) => CreateButton(parent, name, new TranslationReference(translationKey, false), action);

        /// <summary>
        /// Creates a button UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the button</param>
        /// <param name="action">Function called on click</param>
        /// <returns>The button component</returns>
        public static CustomButton CreateButton(Transform parent, string name, TranslationReference translation, UnityAction action)
        {
            var button = Object.Instantiate(Prefabs.LargeButton, parent);
            button.name = name;
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            button.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            button.GetComponent<XDNavigableButton>().onClick.AddListener(CustomButton.PlayButtonSound);
            button.GetComponent<XDNavigableButton>().onClick.AddListener(action);
            return new CustomButton(button);
        }

        /// <summary>
        /// Creates a small toggle UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the toggle</param>
        /// <param name="defaultValue">The toggle's default value</param>
        /// <param name="onValueChange">Function called when the toggle is updated</param>
        /// <returns>The toggle component</returns>
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
        
        /// <summary>
        /// Creates a small toggle UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the toggle</param>
        /// <param name="defaultValue">The toggle's default value</param>
        /// <param name="onValueChange">Function called when the toggle is updated</param>
        /// <returns>The toggle component</returns>
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

        /// <summary>
        /// Creates a large toggle UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the toggle</param>
        /// <param name="defaultValue">The toggle's default value</param>
        /// <param name="onValueChange">Function called when the toggle is updated</param>
        /// <returns>The toggle component</returns>
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

        /// <summary>
        /// Creates a large toggle UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the toggle</param>
        /// <param name="defaultValue">The toggle's default value</param>
        /// <param name="onValueChange">Function called when the toggle is updated</param>
        /// <returns>The toggle component</returns>
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

        /// <summary>
        /// Creates a small multi-choice UI Component based on an enum
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the multi-choice</param>
        /// <param name="defaultValue">The multi-choice's default value</param>
        /// <param name="onValueChange">Function called when the option is changed</param>
        /// <returns>The multi-choice component</returns>
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

        /// <summary>
        /// Creates a small multi-choice UI Component based on an enum
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the multi-choice</param>
        /// <param name="defaultValue">The multi-choice's default value</param>
        /// <param name="onValueChange">Function called when the option is changed</param>
        /// <returns>The multi-choice component</returns>
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

        /// <summary>
        /// Creates a small multi-choice UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the multi-choice</param>
        /// <param name="defaultValue">The multi-choice's default value</param>
        /// <param name="valueChanged">Function called when the option is changed</param>
        /// <param name="valueRangeRequested">Function called to define the multi-choice range</param>
        /// <param name="valueTextRequested">Function called to define each multi-choice option text</param>
        /// <returns>The multi-choice component</returns>
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

        /// <summary>
        /// Creates a small multi-choice UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the multi-choice</param>
        /// <param name="defaultValue">The multi-choice's default value</param>
        /// <param name="valueChanged">Function called when the option is changed</param>
        /// <param name="valueRangeRequested">Function called to define the multi-choice range</param>
        /// <param name="valueTextRequested">Function called to define each multi-choice option text</param>
        /// <returns>The multi-choice component</returns>
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

        /// <summary>
        /// Creates a large multi-choice UI Component based on an enum
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the multi-choice</param>
        /// <param name="defaultValue">The multi-choice's default value</param>
        /// <param name="onValueChange">Function called when the option is changed</param>
        /// <returns>The multi-choice component</returns>
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

        /// <summary>
        /// Creates a large multi-choice UI Component based on an enum
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the multi-choice</param>
        /// <param name="defaultValue">The multi-choice's default value</param>
        /// <param name="onValueChange">Function called when the option is changed</param>
        /// <returns>The multi-choice component</returns>
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

        /// <summary>
        /// Creates a large multi-choice UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translationKey">A translation key for the multi-choice</param>
        /// <param name="defaultValue">The multi-choice's default value</param>
        /// <param name="valueChanged">Function called when the option is changed</param>
        /// <param name="valueRangeRequested">Function called to define the multi-choice range</param>
        /// <param name="valueTextRequested">Function called to define each multi-choice option text</param>
        /// <returns>The multi-choice component</returns>
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

        /// <summary>
        /// Creates a large multi-choice UI Component
        /// </summary>
        /// <param name="parent">A transform to place the component on</param>
        /// <param name="name">The internal name of the component</param>
        /// <param name="translation">A translation reference for the multi-choice</param>
        /// <param name="defaultValue">The multi-choice's default value</param>
        /// <param name="valueChanged">Function called when the option is changed</param>
        /// <param name="valueRangeRequested">Function called to define the multi-choice range</param>
        /// <param name="valueTextRequested">Function called to define each multi-choice option text</param>
        /// <returns>The multi-choice component</returns>
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
            var button = Object.Instantiate(multiChoicePrefab, parent);
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
