using UnityEngine;

namespace SpinCore.UI
{
    public class CustomTextComponent : CustomActiveComponent
    {
        private TranslatedTextMeshPro _tmpText;
        
        internal CustomTextComponent(GameObject obj) : base(obj)
        {
            _tmpText = obj.GetComponentInChildren<TranslatedTextMeshPro>();
        }
        
        public string TextTranslationKey
        {
            get => _tmpText.translation.Key;
            set => _tmpText.SetTranslationKey(value);
        }

        public TranslationReference TextTranslation
        {
            get => _tmpText.translation;
            set => _tmpText.SetTranslation(value);
        }

        public string ExtraText
        {
            get => _tmpText.TextToAppend;
            set => _tmpText.TextToAppend = value;
        }
    }
}
