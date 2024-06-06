using UnityEngine;

namespace SpinCore.UI
{
    public class CustomImage : CustomActiveComponent
    {
        private SlantedRectangle _rect;
        
        internal CustomImage(GameObject obj) : base(obj)
        {
            _rect = obj.GetComponentInChildren<SlantedRectangle>();
        }

        public Texture Texture
        {
            get => _rect.texture;
            set => _rect.texture = value;
        }
    }
}
