using UnityEngine;

namespace SpinCore.UI
{
    public class CustomActiveComponent
    {
        public GameObject GameObject { get; private set; }
        public Transform Transform => GameObject.transform;

        internal CustomActiveComponent(GameObject obj)
        {
            GameObject = obj;
        }

        public bool Active
        {
            get => GameObject.activeSelf;
            set => GameObject.SetActive(value);
        }
    }
}
