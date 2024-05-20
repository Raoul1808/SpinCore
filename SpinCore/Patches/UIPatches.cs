using HarmonyLib;
using SpinCore.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XDMenuPlay;
using XDMenuPlay.Customise;
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

        private static GameObject _customTab;
        private static bool _wantToCreateMenu = false;
        private static bool _shouldRecheckMenu = false;
        private static GameObject _customTabContent;
        private static XDNavigable _customTopNavigable;
        private static XDNavigable _settingsButtonRef;

        private static void CreateMenu()
        {
            var customSection = GameObject.Instantiate(_customTabContent.transform.Find("General Settings Section").gameObject, CustomPrefabStore.RootTransform);
            customSection.name = "CustomSectionPrefab";
            var sectionHeader = GameObject.Instantiate(customSection.transform.GetChild(0).gameObject, CustomPrefabStore.RootTransform);
            sectionHeader.name = "CustomSectionHeader";
            var sectionLine = GameObject.Instantiate(_customTabContent.transform.Find("BuildInfoSection/Line").gameObject, CustomPrefabStore.RootTransform);
            sectionLine.name = "CustomLine";
            for (int i = customSection.transform.childCount; i > 0; i--)
            {
                Object.DestroyImmediate(customSection.transform.GetChild(i - 1).gameObject);
            }
            for (int i = _customTabContent.transform.childCount; i > 0; i--)
            {
                Object.DestroyImmediate(_customTabContent.transform.GetChild(i - 1).gameObject);
            }

            var newSection = GameObject.Instantiate(customSection, _customTabContent.transform);
            newSection.name = "Test Section";
            var newSectionHeader = GameObject.Instantiate(sectionHeader, newSection.transform);
            newSectionHeader.name = "Test Section Header";
            newSectionHeader.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslationKey("SpinCore_CustomTestSection");
            UIHelper.CreateButton(newSection.transform, "Test Button In Settings", "SpinCore_CustomTestButton", () => NotificationSystemGUI.AddMessage(":O"));
            var newSectionLine = GameObject.Instantiate(sectionLine, newSection.transform);
            newSectionLine.name = "Test Line Line Line (fuwa fuwa fuwa fuwa)";
        }
        
        [HarmonyPatch(typeof(XDCustomiseMenu), nameof(XDCustomiseMenu.OnStartupInitialise))]
        [HarmonyPostfix]
        private static void CreateModOptionsPage(XDCustomiseMenu __instance)
        {
            var tabButtonsTransform = __instance.gameObject.transform.Find("VRContainerOffset/TabButtons");
            var settingsTopButton = tabButtonsTransform.Find("CustomiseSettingsButton").gameObject;
            _settingsButtonRef = settingsTopButton.GetComponent<XDNavigable>();
            var customTopButton = GameObject.Instantiate(settingsTopButton, tabButtonsTransform);
            customTopButton.name = "CustomiseModsButton";
            customTopButton.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslationKey("SpinCore_CustomiseModsTabButton");
            _customTopNavigable = customTopButton.GetComponent<XDNavigable>();

            var customiseTabsContainerTransform = __instance.gameObject.transform.Find("VRContainerOffset/MenuContainer/CustomiseTabsContainer");
            var customiseSettingsTab = customiseTabsContainerTransform.Find("CustomiseSettingsTab").gameObject;
            _customTab = GameObject.Instantiate(customiseSettingsTab, customiseTabsContainerTransform);
            _customTab.name = "CustomiseModsTab";
            _customTab.SetActive(false);
            customTopButton.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            customTopButton.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                customTopButton.GetComponent<GameStateUIHelper>().ChangeState(XDCustomiseMenu.StateNames.CustomiseSettings);
                _customTopNavigable.forceExpanded = true;
                customiseSettingsTab.SetActive(false);
                _settingsButtonRef.forceExpanded = false;
                _customTab.SetActive(true);
                if (_customTabContent == null)
                    _wantToCreateMenu = true;
            });
            settingsTopButton.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                HideCustomMenu();
                customiseSettingsTab.SetActive(true);
                _settingsButtonRef.forceExpanded = true;
            });
        }

        [HarmonyPatch(typeof(XDCustomiseMenu), nameof(XDCustomiseMenu.OnGameStateChange))]
        [HarmonyPostfix]
        private static void HideCustomMenu()
        {
            _customTopNavigable.forceExpanded = false;
            _customTab.SetActive(false);
        }

        [HarmonyPatch(typeof(Track), nameof(Track.LateUpdate))]
        [HarmonyPostfix]
        private static void CheckMenuCreation()
        {
            if (_customTab?.activeSelf == true)
            {
                _settingsButtonRef.forceExpanded = false;
            }
            if (!_wantToCreateMenu && !_shouldRecheckMenu) return;
            if (_shouldRecheckMenu)
            {
                Plugin.LogInfo("aaa");
                _shouldRecheckMenu = false;
                _customTabContent = _customTab.transform.Find("Scroll View/Viewport/Content Settings Tab  Prefab(Clone)").gameObject;
                Plugin.LogInfo(_customTabContent);
                Plugin.LogInfo("life is saved");
                CreateMenu();
                return;
            }
            Plugin.LogInfo("Checking menu next frame");
            _wantToCreateMenu = false;
            _shouldRecheckMenu = true;
        }
    }
}
