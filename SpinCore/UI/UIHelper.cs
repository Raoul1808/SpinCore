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
        private const string PanelNamePrefix = "TabPanel_SpinCore";
        internal static GameObject SidePanelButtonBase;
        internal static GameObject SidePanelBase;
        internal static GameObject MultiChoiceBase;
        private static XDTabPanelGroup _tabPanelGroupInstance;

        private static readonly List<CustomSidePanel> SidePanels = new List<CustomSidePanel>();

        internal static void LoadSidePanels(XDTabPanelGroup instance)
        {
            _tabPanelGroupInstance = instance;
            foreach (var panel in SidePanels)
                CreateSidePanelObject(panel);
        }

        internal static void CreateSidePanelObject(CustomSidePanel panel)
        {
            var panelObj = GameObject.Instantiate(SidePanelBase, CustomPrefabStore.RootTransform);
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
            var button = GameObject.Instantiate(SidePanelButtonBase, parent);
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
            var button = GameObject.Instantiate(MultiChoiceBase, parent);
            button.name = name;
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            button.GetComponent<XDNavigableOptionMultiChoice>().state.callbacks = new XDNavigableOptionMultiChoice.Callbacks();
            button.GetComponent<XDNavigableOptionMultiChoice>().SetCallbacksAndValue(defaultValue, valueChanged, valueRangeRequested, valueTextRequested);
            return new CustomMultiChoice(button);
        }
    }
}
