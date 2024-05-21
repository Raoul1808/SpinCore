using UnityEngine;

namespace SpinCore.UI
{
    public class CustomSectionHeader : CustomTextComponent
    {
        private GameObject _spacer;

        public bool Spacer
        {
            get => _spacer.activeSelf;
            set => _spacer.SetActive(value);
        }

        internal CustomSectionHeader(GameObject obj) : base(obj)
        {
            _spacer = obj.transform.Find("Spacer").gameObject;
        }
    }
}
