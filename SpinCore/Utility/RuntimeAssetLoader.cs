using System.IO;
using UnityEngine;

namespace SpinCore.Utility
{
    public static class RuntimeAssetLoader
    {
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
