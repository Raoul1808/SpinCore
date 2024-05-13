using HarmonyLib;
using XDMenuPlay;

namespace SpinCore
{
    internal static class UIPatches
    {
        [HarmonyPatch(typeof(XDTabPanelGroup), nameof(XDTabPanelGroup.SetupTabs))]
        [HarmonyPostfix]
        private static void CreateModSettingsTab(XDTabPanelGroup __instance)
        {
            var tabConfig = new XDTabPanelGroup.TabConfig
            {
                buildFlags = new BuildCondition(),
                name = "Mods",
                prefabs = new XDSelectionListItemDisplay[] {},
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
    }
}
