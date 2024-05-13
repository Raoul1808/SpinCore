using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using XDMenuPlay;
using XDMenuPlay.TrackMenus;

namespace SpinCore.Patches
{
    internal static class UIPatches
    {
        private static GameObject _modPanel;
        private static GameObject _sidePanelButtonBase;
        private static bool _createdButton = false;

        private const string OrigName = "TabPanel_SpinCoreQuickModSettings";
        private const string CloneName = OrigName + "(Clone)";

        private static void CreateModPanel(XDTabPanelGroup instance)
        {
            _modPanel = GameObject.Instantiate(instance.tabs[instance.tabs.Length - 1].prefabs[0].gameObject,
                instance.tabPanelDisplay.transform);
            _modPanel.name = "TabPanel_SpinCoreQuickModSettings";
            Object.Destroy(_modPanel.GetComponent<ManageCustomTracksHandler>());
            var panelContent = _modPanel.transform.Find("Scroll List Tab Prefab/Scroll View/Viewport/Content");
            _sidePanelButtonBase = GameObject.Instantiate(panelContent.Find("ManageTrackPopout/DeleteSelected").gameObject, new GameObject().transform);
            _sidePanelButtonBase.SetActive(false);
            _sidePanelButtonBase.name = "SampleSidePanelButtonAsset";
            for (int i = panelContent.childCount; i > 0; i--)
            {
                Object.DestroyImmediate(panelContent.GetChild(i - 1).gameObject);
            }
        }
        
        [HarmonyPatch(typeof(XDTabPanelGroup), nameof(XDTabPanelGroup.SetupTabs))]
        [HarmonyPostfix]
        private static void CreateModSettingsTab(XDTabPanelGroup __instance)
        {
            CreateModPanel(__instance);
            var tabConfig = new XDTabPanelGroup.TabConfig
            {
                buildFlags = new BuildCondition(),
                name = "Mods",
                prefabs = new [] {_modPanel.GetComponentInChildren<XDSelectionListItemDisplay>()},
                translation = new TranslationReference("SpinCore_ModTab", true),
            };
            XDTabPanelGroup.TabInstance tabInstance = new XDTabPanelGroup.TabInstance
            {
                config = tabConfig,
                translation = new TranslationReference("SpinCore_ModTab", true),
                appendString = "",
            };
            tabInstance.selectorButton = __instance.CreateTabButton(tabInstance);
            tabInstance.selectorButton.transform.SetSiblingIndex(__instance._tabInstances.Count + 1);
            __instance._tabInstances.Add(tabInstance);
            __instance._tabInstancesByName.Add("Mods", tabInstance);
            __instance.UpdateTabInternal();
        }

        [HarmonyPatch(typeof(XDTabPanelGroup), nameof(XDTabPanelGroup.ChangeSelectedTab))]
        [HarmonyPostfix]
        private static void InsertModsButton(XDTabPanelGroup __instance, string tabName)
        {
            if (_createdButton || tabName != "Mods") return;
            var panelContent = __instance.tabPanelDisplay.transform.Find(CloneName + "/Scroll List Tab Prefab/Scroll View/Viewport/Content");
            var button = GameObject.Instantiate(_sidePanelButtonBase, panelContent.transform);
            button.name = "HelloWorld";
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslationKey("SpinCore_ModTabButton");
            button.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            button.GetComponent<XDNavigableButton>().onClick.AddListener(() => { Plugin.LogInfo("Hello, world!"); });
        }
    }
}
