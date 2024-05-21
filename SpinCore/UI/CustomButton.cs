using UnityEngine;
using UnityEngine.Events;

namespace SpinCore.UI
{
    public class CustomButton : CustomTextComponent
    {
        private XDNavigableButton _button;

        internal CustomButton(GameObject button) : base(button)
        {
            _button = button.GetComponent<XDNavigableButton>();
        }

        public void AddListener(UnityAction action) => _button.onClick.AddListener(action);
        public void RemoveListener(UnityAction action) => _button.onClick.RemoveListener(action);
        public void RemoveAllListeners() => _button.onClick.RemoveAllListeners();
    }
}
