using Newtonsoft.Json;

namespace SpinCore.Utility
{
    // I stole this from Speen Chroma, which stole from Dynamic Track Speed, which stole from old SpinCore. Sorry everyone :(
    public static class CustomChartHelper
    {
        /// <summary>
        /// Attempts to get miscellaneous data from a chart file
        /// </summary>
        /// <param name="customFile">The file to read from</param>
        /// <param name="key">The key used to identify the data</param>
        /// <param name="data">The acquired data</param>
        /// <typeparam name="T">The type of the data object</typeparam>
        /// <returns>True if data was found</returns>
        public static bool TryGetCustomData<T>(IMultiAssetSaveFile customFile, string key, out T data) {
            if (!customFile.HasJsonValueForKey(key)) {
                data = default;

                return false;
            }
        
            data = JsonConvert.DeserializeObject<T>(customFile.GetLargeStringOrJson(key).Value);

            return data != null;
        }

        /// <summary>
        /// Saves miscellaneous data to a chart file 
        /// </summary>
        /// <param name="customFile">The file to write to</param>
        /// <param name="key">The key used to identify the data</param>
        /// <param name="data">The data to write</param>
        /// <param name="save">Save the file immediately</param>
        public static void SetCustomData(IMultiAssetSaveFile customFile, string key, object data, bool save = false) {
            customFile.GetLargeStringOrJson(key).Value = JsonConvert.SerializeObject(data);
            customFile.MarkDirty();
        
            if (save)
                customFile.WriteToDiskIfDirty(true);
        }

        /// <summary>
        /// Removes miscellaneous data from a chart file
        /// </summary>
        /// <param name="customFile">The file to remove data from</param>
        /// <param name="key">The key used to identify the data</param>
        /// <param name="save">Save the file immediately</param>
        public static void RemoveCustomData(IMultiAssetSaveFile customFile, string key, bool save = false) {
            if (!customFile.HasJsonValueForKey(key))
                return;
        
            customFile.RemoveJsonValue(key);
            customFile.MarkDirty();
        
            if (save)
                customFile.WriteToDiskIfDirty(true);
        }
    }
}
