using System;
using UnityEngine;

namespace SpinCore.UI
{
    public class CustomInputField : CustomActiveComponent
    {
        public XDNavigableInputField InputField { get; }

        private Action<string, string> _currentListener;
        public Action<string, string> OnValueChanged
        {
            get => _currentListener;
            set
            {
                if (_currentListener != null)
                    InputField.OnValueChanged -= _currentListener;
                _currentListener = value;
                if (_currentListener != null)
                    InputField.OnValueChanged += _currentListener;
            }
        }

        public int CharacterLimit
        {
            get => InputField.tmpInputField.characterLimit;
            set => InputField.tmpInputField.characterLimit = value;
        }

        internal CustomInputField(GameObject obj, Action<string, string> listener) : base(obj)
        {
            InputField = obj.GetComponent<XDNavigableInputField>();
            OnValueChanged = listener;
            CharacterLimit = 255;
        }
    }
}
