using System.IO;
using UnityEngine;

namespace SpinCore.Utility
{
    /// <summary>
    /// A collection of utility functions to load assets at runtime.
    /// </summary>
    public static class RuntimeAssetLoader
    {
        /// <summary>
        /// Loads a Texture2D asset from the given stream. The asset must be a valid image file recognized by Unity.
        /// </summary>
        /// <param name="stream">The stream to load the image from</param>
        /// <returns>The image loaded from the stream</returns>
        public static Texture2D Texture2DFromStream(Stream stream)
        {
            byte[] imageData;
            using (MemoryStream mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                imageData = mem.ToArray();
            }
            var tex = new Texture2D(1, 1);
            tex.LoadImage(imageData);
            return tex;
        }
    }
}
