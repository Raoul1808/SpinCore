using HarmonyLib;
using SpinCore.UI;
using UnityEngine;
using XDMenuPlay;
using XDMenuPlay.TrackMenus;

namespace SpinCore.Patches
{
    internal static class UIPatches
    {
        private static GameObject _modPanel;
        private static bool _loadedTab = false;

        private const string OrigName = "TabPanel_SpinCoreQuickModSettings";
        private const string CloneName = OrigName + "(Clone)";

        private static void CreateModPanel(XDTabPanelGroup instance)
        {
            _modPanel = GameObject.Instantiate(instance.tabs[instance.tabs.Length - 1].prefabs[0].gameObject,
                instance.tabPanelDisplay.transform);
            _modPanel.name = "TabPanel_SpinCoreQuickModSettings";
            Object.Destroy(_modPanel.GetComponent<ManageCustomTracksHandler>());
            var panelContent = _modPanel.transform.Find("Scroll List Tab Prefab/Scroll View/Viewport/Content");
            var sidePanelButtonBase = GameObject.Instantiate(panelContent.Find("ManageTrackPopout/DeleteSelected").gameObject, new GameObject().transform);
            sidePanelButtonBase.SetActive(false);
            sidePanelButtonBase.name = "SampleSidePanelButtonAsset";
            UIHelper.SidePanelButtonBase = sidePanelButtonBase;
            var filterPanelClone = GameObject.Instantiate(instance.tabs[1].prefabs[0].gameObject, new GameObject().transform);
            var multiChoiceBase = GameObject.Instantiate(filterPanelClone.transform.Find("FilterSettingsPopout/TrackSorting").gameObject, new GameObject().transform);
            multiChoiceBase.name = "SampleMultiChoiceButton";
            Object.Destroy(multiChoiceBase.GetComponent<XDNavigableOptionMultiChoice_IntValue>());
            multiChoiceBase.GetComponent<XDNavigableOptionMultiChoice>().state.callbacks = new XDNavigableOptionMultiChoice.Callbacks();
            multiChoiceBase.GetComponentInChildren<TranslatedTextMeshPro>().translation = TranslationReference.Empty;
            multiChoiceBase.SetActive(false);
            UIHelper.MultiChoiceBase = multiChoiceBase;
            Object.Destroy(filterPanelClone);
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
            if (_loadedTab || tabName != "Mods") return;
            UIHelper.CallSidePanelEvent(__instance.tabPanelDisplay.transform.Find(CloneName + "/Scroll List Tab Prefab/Scroll View/Viewport/Content"));
            _loadedTab = true;
        }
    }
}
