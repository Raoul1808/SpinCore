using HarmonyLib;
using SpinCore.Triggers;

namespace SpinCore.Patches
{
    [HarmonyPatch]
    internal static class TriggerPatches
    {
        [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        [HarmonyPostfix]
        private static void UpdateTriggers()
        {
            if (Track.PlayStates.Length == 0)
                return;
            var playStateFirst = Track.PlayStates[0];
            TriggerManager.Update(playStateFirst.currentTrackTime);
        }

        [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack))]
        [HarmonyPostfix]
        private static void ChartPlay()
        {
            TriggerManager.ResetTriggerStores();
        }

        [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack))]
        [HarmonyPostfix]
        private static void ReturnToPickTrack()
        {
            TriggerManager.ClearAllTriggers();
        }
    }
}
