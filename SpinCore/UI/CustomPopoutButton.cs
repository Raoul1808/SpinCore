using UnityEngine;

namespace SpinCore.UI
{
    public class CustomPopoutButton : CustomButton
    {
        public static CustomPage Page { get; private set; }
        
        internal CustomPopoutButton(GameObject button, CustomPage page) : base(button)
        {
            Page = page;
        }
    }
}
