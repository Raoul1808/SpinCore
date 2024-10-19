using UnityEngine;

namespace SpinCore
{
    internal static class CustomPrefabStore
    {
        public static GameObject RootGameObject { get; }
        public static Transform RootTransform => RootGameObject.transform;

        static CustomPrefabStore()
        {
            RootGameObject = new GameObject
            {
                name = "SpinCorePrefabStore"
            };
            RootGameObject.SetActive(false);
        }
    }
}