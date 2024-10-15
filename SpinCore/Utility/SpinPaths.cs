using System.Collections.Generic;

namespace SpinCore.Utility
{
    public static class SpinPaths
    {
        public static string GetAlbumArtForChart(MetadataHandle handle)
        {
            return handle.albumArtRef.customFile.FilePath;
        }

        public static string GetSrtbForChart(MetadataHandle handle)
        {
            return handle.TrackInfoRef.customFile.FilePath;
        }

        public static string[] GetClipsForChart(MetadataHandle handle)
        {
            var clips = new List<string>();
            foreach (var trackDataRef in handle.trackDataMetadata.trackDataRefs)
            {
                foreach (var clipInfoRef in trackDataRef.asset.clipInfoAssetReferences)
                {
                    string path = clipInfoRef.customFile?.FilePath;
                    if (string.IsNullOrEmpty(path) || clips.Contains(path)) continue;
                    clips.Add(path);
                }
            }

            return clips.ToArray();
        }

        public static string GetTriggerPathForChart(MetadataHandle handle, string extension)
        {
            string path = handle.TrackInfoRef.customFile?.FilePath ?? string.Empty;
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.Substring(0, path.Length - 4) + "." + extension;
        }

        public static string GetTriggerPathForChartAndDifficulty(MetadataHandle handle, TrackData.DifficultyType difficulty, string extension)
        {
            string path = handle.TrackInfoRef.customFile?.FilePath ?? string.Empty;
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.Substring(0, path.Length - 4) + "_" + difficulty.ToString().ToUpper() + "." + extension;
        }
    }
}
