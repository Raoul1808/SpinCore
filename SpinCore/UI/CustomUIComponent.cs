using UnityEngine;

namespace SpinCore.UI
{
    public abstract class CustomUIComponent
    {
        public GameObject GameObject { get; private set; }
        private TranslatedTextMeshPro _tmpText;
        
        internal CustomUIComponent(GameObject obj)
        {
            GameObject = obj;
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
            set => _tmpText.translation = value;
        }

        public string ExtraText
        {
            get => _tmpText.TextToAppend;
            set => _tmpText.TextToAppend = value;
        }
    }
}
