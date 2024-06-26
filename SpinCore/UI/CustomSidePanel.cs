using UnityEngine;

namespace SpinCore.UI
{
    public class CustomSidePanel
    {
        internal bool Loaded { get; private set; }
        public Transform PanelTransform { get; internal set; }
        public Transform PanelContentTransform { get; internal set; }
        public string PanelName { get; private set; }
        public TranslationReference Translation { get; private set; }
        public Sprite Sprite { get; internal set; }
        
        internal CustomSidePanel(string name, TranslationReference translation, Sprite sprite)
        {
            PanelName = name;
            Translation = translation;
            Sprite = sprite;
        }

        public delegate void SidePanelLoad(Transform panelTransform);

        public event SidePanelLoad OnSidePanelLoaded;

        internal void OnSidePanelFocus()
        {
            if (Loaded) return;
            OnSidePanelLoaded?.Invoke(PanelContentTransform);
            Loaded = true;
        }
    }
}
