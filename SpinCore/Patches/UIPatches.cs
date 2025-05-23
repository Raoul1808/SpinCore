using HarmonyLib;
using SpinCore.UI;
using UnityEngine;
using UnityEngine.UI;
using XDMenuPlay;
using XDMenuPlay.Customise;
using XDMenuPlay.TrackMenus;

namespace SpinCore.Patches
{
    [HarmonyPatch]
    internal static class UIPatches
    {
        private static XDMainMenu _mainMenuInstance;
        
        private static void GetRequiredPanelUIComponents(XDTabPanelGroup instance)
        {
            var modPanel = Object.Instantiate(instance.tabs[instance.tabs.Length - 1].prefabs[0].gameObject, CustomPrefabStore.RootTransform);
            modPanel.name = "TabPanel_SpinCoreBasePanel";
            modPanel.SetActive(false);
            Object.Destroy(modPanel.GetComponent<ManageCustomTracksHandler>());
            var panelContent = modPanel.transform.Find("Scroll List Tab Prefab/Scroll View/Viewport/Content");
            var sidePanelButtonBase = Object.Instantiate(panelContent.Find("ManageTrackPopout/DeleteSelected").gameObject, CustomPrefabStore.RootTransform);
            sidePanelButtonBase.SetActive(false);
            sidePanelButtonBase.name = "SampleSidePanelButtonAsset";
            UIHelper.Prefabs.LargeButton = sidePanelButtonBase;

            // var panelLabel = GameObject.Instantiate(panelContent.Find("DescriptonContainer/CreateTrackDescription").gameObject, CustomPrefabStore.RootTransform);
            // panelLabel.name = "SampleLabel";
            // UIHelper.Prefabs.Label = panelLabel;

            var panelImage = Object.Instantiate(panelContent.Find("DescriptonContainer/ImageContainer").gameObject, CustomPrefabStore.RootTransform);
            panelImage.name = "SampleImage";
            panelImage.GetComponentInChildren<LayoutElement>().ignoreLayout = false;
            UIHelper.Prefabs.Image = panelImage;

            var filterPanelClone = Object.Instantiate(instance.tabs[1].prefabs[0].gameObject, CustomPrefabStore.RootTransform);
            var smallMultiChoice = Object.Instantiate(filterPanelClone.transform.Find("Scroll List Tab Prefab/Scroll View/Viewport/Content/FilterSettingsPopout/TrackSorting").gameObject, CustomPrefabStore.RootTransform);
            smallMultiChoice.GetComponent<XDNavigableOptionMultiChoice>().state.callbacks = new XDNavigableOptionMultiChoice.Callbacks();
            smallMultiChoice.GetComponentInChildren<TranslatedTextMeshPro>().translation = TranslationReference.Empty;
            Object.Destroy(smallMultiChoice.GetComponent<XDNavigableOptionMultiChoice_IntValue>());
            smallMultiChoice.name = "SampleSmallMultiChoice";
            UIHelper.Prefabs.SmallMultiChoice = smallMultiChoice;
            Object.Destroy(filterPanelClone);

            panelContent.RemoveAllChildren();
            UIHelper.Prefabs.SidePanel = modPanel;
        }

        [HarmonyPatch(typeof(XDTabPanelGroup), nameof(XDTabPanelGroup.SetupTabs))]
        [HarmonyPostfix]
        private static void PrepareUIHelper(XDTabPanelGroup __instance)
        {
            GetRequiredPanelUIComponents(__instance);
            UIHelper.LoadSidePanels(__instance);
        }

        [HarmonyPatch(typeof(XDTabPanelGroup), nameof(XDTabPanelGroup.ChangeSelectedTab))]
        [HarmonyPostfix]
        private static void CallSidePanelEvent(XDTabPanelGroup __instance, string tabName)
        {
            UIHelper.CallSidePanelEvent(tabName);
        }

        private static XDNavigable _landscapeCustomTopNavigable;
        private static XDNavigable _portraitCustomTopNavigable;
        private static XDNavigable _landscapeSettingsButtonRef;
        private static XDNavigable _portraitSettingsButtonRef;
        private static CustomPage _modSettingsPage;

        private static void PrepareMenuPrefabs(XDCustomiseMenu instance)
        {
            var settingsTab = instance.gameObject.transform.Find("VRContainerOffset/MenuContainer/CustomiseTabsContainer/CustomiseSettingsTab").gameObject;
            var tabBase = Object.Instantiate(settingsTab, CustomPrefabStore.RootTransform);
            var settingsTabLoadPrefabComponent = tabBase.GetComponent<LoadPrefabOnStart>();
            var settingsTabContentPrefab = settingsTabLoadPrefabComponent.prefabToLoad;
            Object.DestroyImmediate(settingsTabLoadPrefabComponent);
            var tabContentBase = Object.Instantiate(settingsTabContentPrefab, tabBase.transform.Find("Scroll View/Viewport/"));
            tabContentBase.name = "Content";
            var tabSectionBase = Object.Instantiate(tabContentBase.transform.Find("General Settings Section").gameObject, CustomPrefabStore.RootTransform);
            tabSectionBase.name = "CustomSectionPrefab";
            var sectionHeader = Object.Instantiate(tabSectionBase.transform.GetChild(0).gameObject, CustomPrefabStore.RootTransform);
            sectionHeader.name = "CustomSectionHeader";
            var sectionLine = Object.Instantiate(tabContentBase.transform.Find("BuildInfoSection/Line").gameObject, CustomPrefabStore.RootTransform);
            sectionLine.name = "CustomLine";
            var popoutButton = Object.Instantiate(tabContentBase.transform.Find("General Settings Section/CalibrationButton").gameObject, CustomPrefabStore.RootTransform);
            popoutButton.name = "CustomPopoutButton";
            popoutButton.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();

            var multiChoiceBase = Object.Instantiate(tabSectionBase.transform.Find("Language").gameObject, CustomPrefabStore.RootTransform);
            multiChoiceBase.name = "SampleMultiChoiceButton";
            Object.DestroyImmediate(multiChoiceBase.GetComponent<XDNavigableOptionMultiChoice_IntValue>());
            Object.DestroyImmediate(multiChoiceBase.GetComponent<LanguageMultiChoiceHandler>());
            multiChoiceBase.GetComponent<XDNavigableOptionMultiChoice>().state.callbacks = new XDNavigableOptionMultiChoice.Callbacks();
            multiChoiceBase.GetComponentInChildren<TranslatedTextMeshPro>().translation = TranslationReference.Empty;
            multiChoiceBase.SetActive(false);
            UIHelper.Prefabs.LargeMultiChoice = multiChoiceBase;
            var optionLabel = Object.Instantiate(multiChoiceBase.transform.Find("OptionLabel").gameObject, CustomPrefabStore.RootTransform);

            var gamepadControlStyle = tabContentBase.transform.Find("Content Settings Section Simple Input/GamepadControlStyle").gameObject;
            var tooltipOpener = gamepadControlStyle.GetComponent<XDTooltipPopoutOpener>();
            UIHelper.Prefabs.TooltipPopout = tooltipOpener.tooltipPrefab;

            tabSectionBase.transform.RemoveAllChildren();
            tabContentBase.transform.RemoveAllChildren();

            UIHelper.Prefabs.Label = optionLabel;
            UIHelper.Prefabs.Line = sectionLine;
            UIHelper.Prefabs.EmptySection = tabSectionBase;
            UIHelper.Prefabs.SectionHeader = sectionHeader;
            UIHelper.Prefabs.SettingsPage = tabBase;
            UIHelper.Prefabs.PopoutButton = popoutButton;
        }

        private static void CreateModSettingsButton(XDCustomiseMenu instance)
        {
            var tabButtonsTransform = instance.gameObject.transform.Find("VRContainerOffset/TabButtons");
            var customiseTabsContainerTransform = instance.gameObject.transform.Find("VRContainerOffset/MenuContainer/CustomiseTabsContainer");
            var settingsTopButton = tabButtonsTransform.Find("CustomiseSettingsButton").gameObject;
            var customiseSettingsTab = customiseTabsContainerTransform.Find("CustomiseSettingsTab").gameObject;
            _landscapeSettingsButtonRef = settingsTopButton.GetComponent<XDNavigable>();
            var customTopButton = Object.Instantiate(settingsTopButton, tabButtonsTransform);
            customTopButton.transform.SetSiblingIndex(5);
            customTopButton.name = "CustomiseModsButton";
            customTopButton.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslationKey("SpinCore_CustomiseModsTabButton");
            _landscapeCustomTopNavigable = customTopButton.GetComponent<XDNavigable>();

            _modSettingsPage = UIHelper.InitializeCustomModPage();

            customTopButton.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            customTopButton.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                if (UIHelper.AnyPagesLeftOnStack())
                    UIHelper.ClearStack();
                _mainMenuInstance.gameObject.GetComponent<GameStateUIHelper>().ChangeState(XDCustomiseMenu.StateNames.CustomiseSettings);
                _landscapeCustomTopNavigable.forceExpanded = true;
                customiseSettingsTab.SetActive(false);
                _landscapeSettingsButtonRef.forceExpanded = false;
                UIHelper.PushPageOnStack(_modSettingsPage);
            });
            settingsTopButton.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                HideCustomMenu();
                customiseSettingsTab.SetActive(true);
                _landscapeSettingsButtonRef.forceExpanded = true;
            });

            var backButton = instance.transform.Find("VRContainerOffset/BackButtonContainer/BackButton Variant").gameObject;
            backButton.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            backButton.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                UIHelper.RemoveLastFromStack();
                if (!UIHelper.AnyPagesLeftOnStack())
                    instance.ExitButtonPressed();
            });
        }

        private static void CreateModSettingsButtonNoText(XDCustomiseMenu instance)
        {
            var tabButtonsTransform = instance.gameObject.transform.Find("VRContainerOffset/TabButtons");
            var customiseTabsContainerTransform = instance.gameObject.transform.Find("VRContainerOffset/MenuContainer/CustomiseTabsContainer");
            var settingsTopButton = tabButtonsTransform.Find("CustomiseSettingsButtonNoText").gameObject;
            var customiseSettingsTab = customiseTabsContainerTransform.Find("CustomiseSettingsTab").gameObject;
            _portraitSettingsButtonRef = settingsTopButton.GetComponent<XDNavigable>();
            var customTopButton = Object.Instantiate(settingsTopButton, tabButtonsTransform);
            customTopButton.name = "CustomiseModsButtonNoText";
            customTopButton.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslationKey("SpinCore_CustomiseModsTabButtonNoText");
            _portraitCustomTopNavigable = customTopButton.GetComponent<XDNavigable>();

            _modSettingsPage = UIHelper.InitializeCustomModPage();

            customTopButton.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            customTopButton.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                if (UIHelper.AnyPagesLeftOnStack())
                    UIHelper.ClearStack();
                _mainMenuInstance.gameObject.GetComponent<GameStateUIHelper>().ChangeState(XDCustomiseMenu.StateNames.CustomiseSettings);
                _portraitCustomTopNavigable.forceExpanded = true;
                customiseSettingsTab.SetActive(false);
                _portraitSettingsButtonRef.forceExpanded = false;
                UIHelper.PushPageOnStack(_modSettingsPage);
            });
            settingsTopButton.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                HideCustomMenu();
                customiseSettingsTab.SetActive(true);
                _portraitSettingsButtonRef.forceExpanded = true;
            });

            var backButton = instance.transform.Find("VRContainerOffset/BackButtonContainer/BackButton Variant").gameObject;
            backButton.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            backButton.GetComponent<XDNavigableButton>().onClick.AddListener(() =>
            {
                UIHelper.RemoveLastFromStack();
                if (!UIHelper.AnyPagesLeftOnStack())
                    instance.ExitButtonPressed();
            });
        }

        [HarmonyPatch(typeof(XDCustomiseMenu), nameof(XDCustomiseMenu.OnStartupInitialise))]
        [HarmonyPostfix]
        private static void CreateModOptionsPage(XDCustomiseMenu __instance)
        {
            var customiseTabsContainerTransform = __instance.gameObject.transform.Find("VRContainerOffset/MenuContainer/CustomiseTabsContainer");
            UIHelper.CommonTabParent = customiseTabsContainerTransform;
            PrepareMenuPrefabs(__instance);
            CreateModSettingsButton(__instance);
            CreateModSettingsButtonNoText(__instance);
            UIHelper.CreateCustomPagesInBuffer();
        }

        [HarmonyPatch(typeof(XDCustomiseMenu), nameof(XDCustomiseMenu.OnGameStateChange))]
        [HarmonyPostfix]
        private static void HideCustomMenu()
        {
            UIHelper.ClearStack();
            _landscapeCustomTopNavigable.forceExpanded = false;
            _portraitCustomTopNavigable.forceExpanded = false;
            _modSettingsPage.Active = false;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.LateUpdate))]
        [HarmonyPostfix]
        private static void ApplyOptionsVisualFix()
        {
            if (UIHelper.AnyPagesLeftOnStack())
            {
                _landscapeSettingsButtonRef.forceExpanded = false;
                _portraitSettingsButtonRef.forceExpanded = false;
            }
        }

        [HarmonyPatch(typeof(XDSelectionListMenu), nameof(XDSelectionListMenu.OnStartupInitialise))]
        [HarmonyPostfix]
        private static void CloneSearchBar(XDSelectionListMenu __instance)
        {
            var searchBox = __instance.gameObject.transform.Find("Container/Content/MainContentMoveAmount/HorizontalMover/MainContentContainer/MainSelectionPanel/SearchButtonsContainer/SearchButtonsOffset/SearchFieldContainer/SearchFilter");
            var inputFieldClone = Object.Instantiate(searchBox.gameObject, CustomPrefabStore.RootTransform);
            inputFieldClone.GetComponent<XDNavigableInputField>().pointSize = 32;
            inputFieldClone.transform.Find("Text Area").localPosition = new Vector3(24, 0, 0);
            inputFieldClone.transform.Find("IconContainer").gameObject.SetActive(false);
            UIHelper.Prefabs.InputField = inputFieldClone;
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Update))]
        [HarmonyPrefix]
        private static void RunSidePanelBuffer()
        {
            UIHelper.CheckPanelCreation();
        }

        [HarmonyPatch(typeof(XDMainMenu), nameof(XDMainMenu.Update))]
        [HarmonyPostfix]
        private static void SaveMenuReference(XDMainMenu __instance)
        {
            if (_mainMenuInstance == null)
            {
                _mainMenuInstance = __instance;
            }
        }

        [HarmonyPatch(typeof(ModalMessageDialog), nameof(ModalMessageDialog.CloseMenu))]
        [HarmonyPostfix]
        private static void RemoveCustomUI()
        {
            ModalMessageDialogExtensions.ModalMessageDialogClosed();
        }
    }
}
