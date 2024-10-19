using System.Collections.Generic;

namespace SpinCore.Utility
{
    /// <summary>
    /// A collection of utility functions to get system paths to external assets used by the game, mostly in the custom charts area.
    /// </summary>
    public static class SpinPaths
    {
        /// <summary>
        /// Gets the path to the album art for the given chart metadata.
        /// </summary>
        /// <param name="handle">The chart's metadata handle</param>
        /// <returns>The path to the album art, or null if it doesn't exist</returns>
        public static string GetAlbumArtForChart(MetadataHandle handle)
        {
            return handle.albumArtRef.customFile.FilePath;
        }

        /// <summary>
        /// Gets the path to the srtb file for the given chart metadata.
        /// </summary>
        /// <param name="handle">The chart's metadata handle</param>
        /// <returns>The path to the srtb file, or null if it doesn't exist</returns>
        public static string GetSrtbForChart(MetadataHandle handle)
        {
            return handle.TrackInfoRef.customFile.FilePath;
        }

        /// <summary>
        /// Gets a collection of audio clips used by the given chart.
        /// </summary>
        /// <param name="handle">The chart's metadata handle</param>
        /// <returns>A collection of audio clips used</returns>
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

        /// <summary>
        /// Gets the path to the global triggers file for the given chart and trigger file extension.
        /// </summary>
        /// <param name="handle">The chart's metadata handle</param>
        /// <param name="extension">The triggers file extension (without a preceding period)</param>
        /// <returns>A constructed path to the triggers file. This file may or may not exist.</returns>
        public static string GetTriggerPathForChart(MetadataHandle handle, string extension)
        {
            string path = handle.TrackInfoRef.customFile?.FilePath ?? string.Empty;
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.Substring(0, path.Length - 4) + "." + extension;
        }

        /// <summary>
        /// Gets the path to the difficulty-specific triggers file for the given chart, difficulty and trigger file extension.
        /// </summary>
        /// <param name="handle">The chart's metadata handle</param>
        /// <param name="difficulty">The chart's difficulty</param>
        /// <param name="extension">The triggers file extension (without a preceding period)</param>
        /// <returns>A constructed path to the triggers file. This file may or may not exist.</returns>
        public static string GetTriggerPathForChartAndDifficulty(MetadataHandle handle, TrackData.DifficultyType difficulty, string extension)
        {
            string path = handle.TrackInfoRef.customFile?.FilePath ?? string.Empty;
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.Substring(0, path.Length - 4) + "_" + difficulty.ToString().ToUpper() + "." + extension;
        }
    }
}
