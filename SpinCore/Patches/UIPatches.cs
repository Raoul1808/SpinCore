using HarmonyLib;
using SpinCore.UI;
using UnityEngine;
using XDMenuPlay;
using XDMenuPlay.TrackMenus;

namespace SpinCore.Patches
{
    internal static class UIPatches
    {
        private static void GetRequiredUIComponents(XDTabPanelGroup instance)
        {
            var modPanel = GameObject.Instantiate(instance.tabs[instance.tabs.Length - 1].prefabs[0].gameObject, CustomPrefabStore.RootTransform);
            modPanel.name = "TabPanel_SpinCoreBasePanel";
            modPanel.SetActive(false);
            Object.Destroy(modPanel.GetComponent<ManageCustomTracksHandler>());
            var panelContent = modPanel.transform.Find("Scroll List Tab Prefab/Scroll View/Viewport/Content");
            var sidePanelButtonBase = GameObject.Instantiate(panelContent.Find("ManageTrackPopout/DeleteSelected").gameObject, CustomPrefabStore.RootTransform);
            sidePanelButtonBase.SetActive(false);
            sidePanelButtonBase.name = "SampleSidePanelButtonAsset";
            UIHelper.SidePanelButtonBase = sidePanelButtonBase;
            var filterPanelClone = GameObject.Instantiate(instance.tabs[1].prefabs[0].gameObject, new GameObject().transform);
            var multiChoiceBase = GameObject.Instantiate(filterPanelClone.transform.Find("FilterSettingsPopout/TrackSorting").gameObject, CustomPrefabStore.RootTransform);
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
            UIHelper.SidePanelBase = modPanel;
        }

        [HarmonyPatch(typeof(XDTabPanelGroup), nameof(XDTabPanelGroup.SetupTabs))]
        [HarmonyPostfix]
        private static void PrepareUIHelper(XDTabPanelGroup __instance)
        {
            GetRequiredUIComponents(__instance);
            UIHelper.LoadSidePanels(__instance);
        }

        [HarmonyPatch(typeof(XDTabPanelGroup), nameof(XDTabPanelGroup.ChangeSelectedTab))]
        [HarmonyPostfix]
        private static void CallSidePanelEvent(XDTabPanelGroup __instance, string tabName)
        {
            UIHelper.CallSidePanelEvent(tabName);
        }
    }
}
