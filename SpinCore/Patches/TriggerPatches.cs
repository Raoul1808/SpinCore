using HarmonyLib;
using SpinCore.Triggers;

namespace SpinCore.Patches
{
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

        [HarmonyPatch(typeof(SplineTrackData.DataToGenerate), MethodType.Constructor, typeof(PlayableTrackData))]
        [HarmonyPostfix]
        private static void ChartLoaded(PlayableTrackData trackData)
        {
            // TODO: find a better patch than this
            TriggerManager.ClearAllTriggers();
            if (trackData.TrackDataList.Count == 0)
                return;
            var data = trackData.TrackDataList[0];
            string path = data.CustomFile?.FilePath;
            if (string.IsNullOrEmpty(path))
                return;
            TriggerManager.InvokeChartLoadEvent(data);
        }
    }
}
