using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SpinCore.Translation;

namespace SpinCore.Patches
{
    [HarmonyPatch]
    internal static class TranslationPatches
    {
        [HarmonyPatch(typeof(TranslationSystem), nameof(TranslationSystem.InitSettings_Internal))]
        [HarmonyPrefix]
        private static void PrepareTranslationKeys(TranslationSystem __instance)
        {
            TranslationHelper.AddAllPendingKeys();
            LanguageHelper.AddPendingLanguages();
        }

        private static int _language;

        [HarmonyPatch(typeof(TranslationSystem), nameof(TranslationSystem.CalculateCurrentLanguage))]
        [HarmonyPrefix]
        private static void YoinkTheLanguage()
        {
            if (PlayerSettingsData.Instance == null)
                return;
            //SpinCorePlugin.LogInfo("I am yoink language");
            _language = PlayerSettingsData.Instance.CurrentLanguageIndex.GetValue();
            //SpinCorePlugin.LogInfo("Language was " + (SupportedLanguage)_language);
        }

        [HarmonyPatch(typeof(TranslationSystem), nameof(TranslationSystem.CalculateCurrentLanguage))]
        [HarmonyPostfix]
        private static void OverrideTheLanguage(ref SupportedLanguage __result)
        {
            //SpinCorePlugin.LogInfo("Language was set to " + __result);
            if (_language >= 15)
            {
                //SpinCorePlugin.LogInfo("I am replace language");
                __result = (SupportedLanguage)_language;
            }
            //SpinCorePlugin.LogInfo("Language is now " + __result);
        }

        [HarmonyPatch(typeof(TranslationSystem), nameof(TranslationSystem.CycleLanguage))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ReplaceLanguageCount(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                SpinCorePlugin.LogInfo("Looking at opcode " + codes[i].opcode + " " + codes[i].operand + " " + codes[i].operand?.GetType());
                if (codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand is sbyte operand && (sbyte?)operand == 15)
                {
                    codes[i] = new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(LanguageHelper), nameof(LanguageHelper.LanguageCount)));
                    SpinCorePlugin.LogInfo("I am replace opcode!!! " + codes[i].opcode + " " + codes[i].operand + " " + codes[i].operand?.GetType());
                }
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(TranslationSystem), nameof(TranslationSystem.CycleLanguage))]
        [HarmonyPostfix]
        private static void SeeLanguage()
        {
            SpinCorePlugin.LogInfo("Language set to " + (SupportedLanguage)PlayerSettingsData.Instance.CurrentLanguageIndex.GetValue());
        }
    }
}
