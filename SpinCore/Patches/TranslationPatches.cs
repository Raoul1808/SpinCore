using HarmonyLib;

namespace SpinCore.Patches
{
    internal static class TranslationPatches
    {
        [HarmonyPatch(typeof(TranslationSystem), nameof(TranslationSystem.GenerateTranslationLookupIfNeeded))]
        [HarmonyPrefix]
        private static void PrepareTranslationKeys(TranslationSystem __instance)
        {
            TranslationHelper.AddAllPendingKeys();
        }
    }
}
