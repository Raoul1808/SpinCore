using System;
using System.Collections.Generic;
using System.Linq;
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
            public GameObject MultiChoice { get; internal set; }
            public GameObject SettingsPage { get; internal set; }
            public GameObject Line { get; internal set; }
            public GameObject SectionHeader { get; internal set; }
            public GameObject EmptySection { get; internal set; }
            public GameObject PopoutButton { get; internal set; }
            
            internal UIPrefabs() {}
        }

        public static UIPrefabs Prefabs { get; } = new UIPrefabs();

        private const string PanelNamePrefix = "TabPanel_SpinCore";
        private static XDTabPanelGroup _tabPanelGroupInstance;

        private static readonly List<CustomSidePanel> SidePanels = new List<CustomSidePanel>();
        private static readonly List<CustomPage> PageStack = new List<CustomPage>();

        private static Transform _commonTabParent;

        internal static void SetTabParent(Transform baseTransform)
        {
            _commonTabParent = baseTransform;
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
            try
            {
                sidePanel = SidePanels.Find(panel => panel.PanelName == panelName);
            }
            catch (ArgumentNullException)
            {
                return;
            }

            // FIXME: This bit of code is here to prevent a NullReferenceException due to some weird shenanigans.
            // TODO: Fix above
            if (!sidePanel.Preloaded)
            {
                sidePanel.Preload();
                XDSelectionListMenu.Instance.CloseSidePanel();
                return;
            }

            if (!sidePanel.Loaded)
            {
                _tabPanelGroupInstance.UpdateTabInternal();
                sidePanel.PanelTransform = _tabPanelGroupInstance.tabPanelDisplay.transform.Find(PanelNamePrefix + sidePanel.PanelName + "(Clone)");
                sidePanel.PanelContentTransform = sidePanel.PanelTransform.Find("Scroll List Tab Prefab/Scroll View/Viewport/Content");
                sidePanel.OnSidePanelFocus();
            }
        }

        internal static void PushPageOnStack(CustomPage page)
        {
            Plugin.LogInfo("Try add " + page.PageName);
            if (PageStack.Contains(page)) return;
            Plugin.LogInfo("Adding page " + page.PageName);

            if (PageStack.Count > 0)
                PageStack[PageStack.Count - 1].Active = false;
            page.Active = true;
            page.OnFocus();
            PageStack.Add(page);
        }

        internal static void ClearStack()
        {
            Plugin.LogInfo("Clearing page stack");
            if (PageStack.Count > 0)
                PageStack[PageStack.Count - 1].Active = false;
            PageStack.Clear();
        }

        internal static void RemoveLastFromStack()
        {
            if (PageStack.Count > 0)
            {
                Plugin.LogInfo("Removing last from stack");
                PageStack[PageStack.Count - 1].Active = false;
                PageStack.RemoveAt(PageStack.Count - 1);
            }
        }

        internal static bool AnyPagesLeftOnStack() => PageStack.Count > 0;

        public static CustomPage CreateSettingsPage(string name)
        {
            var customPage = GameObject.Instantiate(Prefabs.SettingsPage, _commonTabParent);
            customPage.name = "SpinCoreCustomTab_" + name;
            var contentTransform = customPage.transform.Find("Scroll View/Viewport/Content");
            return new CustomPage(name)
            {
                Active = false,
                GameObject = customPage,
                PageTransform = customPage.transform,
                PageContentTransform = contentTransform,
            };
        }

        public static CustomActiveComponent CreateSection(Transform parent, string name, Action<Transform> createContent)
        {
            var section = GameObject.Instantiate(Prefabs.EmptySection, parent);
            section.name = name;
            createContent.Invoke(section.transform);
            return new CustomActiveComponent(section);
        }

        public static CustomSimpleText CreateSectionHeader(Transform parent, string name, string translationKey) => CreateSectionHeader(parent, name, new TranslationReference(translationKey, false));

        public static CustomSimpleText CreateSectionHeader(Transform parent, string name, TranslationReference translation)
        {
            var header = GameObject.Instantiate(Prefabs.SectionHeader, parent);
            header.name = name;
            header.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            return new CustomSimpleText(header);
        }

        public static GameObject CreateLine(Transform parent, string name)
        {
            var line = GameObject.Instantiate(Prefabs.Line, parent);
            line.name = name;
            return line;
        }

        public static CustomSidePanel CreateSidePanel(string name, string translationKey) => CreateSidePanel(name, new TranslationReference(translationKey, false));
        public static CustomSidePanel CreateSidePanel(string name, TranslationReference translation)
        {
            var panel = new CustomSidePanel(name, translation);
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

        public static CustomMultiChoice CreateToggle(
            Transform parent,
            string name,
            string translationKey,
            bool defaultValue,
            Action<bool> onValueChange)
        {
            return CreateToggle(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                onValueChange
            );
        }
        
        public static CustomMultiChoice CreateToggle(
            Transform parent,
            string name,
            TranslationReference translation,
            bool defaultValue,
            Action<bool> onValueChange)
        {
            return CreateMultiChoiceButton(
                parent,
                name,
                translation,
                defaultValue ? 1 : 0,
                v => onValueChange.Invoke(v == 1),
                () => new IntRange(0, 2),
                v => v == 1 ? "UI_Yes" : "UI_No"
            );
        }

        public static CustomMultiChoice CreateMultiChoiceButton<T>(
            Transform parent,
            string name,
            string translationKey,
            T defaultValue,
            Action<T> onValueChange) where T : Enum
        {
            return CreateMultiChoiceButton(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                onValueChange
            );
        }
        
        public static CustomMultiChoice CreateMultiChoiceButton<T>(
            Transform parent,
            string name,
            TranslationReference translation,
            T defaultValue,
            Action<T> onValueChange) where T : Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));
            int defaultValueInt = values.ToList().IndexOf(defaultValue);
            return CreateMultiChoiceButton(
                parent,
                name,
                translation,
                defaultValueInt,
                v => onValueChange.Invoke(values[v]),
                () => new IntRange(0, values.Length),
                v => values[v].ToString()
            );
        }

        public static CustomMultiChoice CreateMultiChoiceButton(
            Transform parent,
            string name,
            string translationKey,
            int defaultValue,
            XDNavigableOptionMultiChoice.OnValueChanged valueChanged,
            XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested,
            XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested)
        {
            return CreateMultiChoiceButton(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                valueChanged,
                valueRangeRequested,
                valueTextRequested
            );
        }
        
        public static CustomMultiChoice CreateMultiChoiceButton(
            Transform parent,
            string name,
            TranslationReference translation,
            int defaultValue,
            XDNavigableOptionMultiChoice.OnValueChanged valueChanged,
            XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested,
            XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested)
        {
            var button = GameObject.Instantiate(Prefabs.MultiChoice, parent);
            button.name = name;
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            button.GetComponent<XDNavigableOptionMultiChoice>().state.callbacks = new XDNavigableOptionMultiChoice.Callbacks();
            button.GetComponent<XDNavigableOptionMultiChoice>().SetCallbacksAndValue(defaultValue, valueChanged, valueRangeRequested, valueTextRequested);
            return new CustomMultiChoice(button);
        }
    }
}
