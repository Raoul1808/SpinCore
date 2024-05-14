using UnityEngine;

namespace SpinCore.UI
{
    public class CustomSidePanel
    {
        internal bool Loaded { get; private set; }
        internal bool Preloaded { get; private set; }
        public Transform PanelTransform { get; internal set; }
        public Transform PanelContentTransform { get; internal set; }
        public string PanelName { get; private set; }
        public TranslationReference Translation { get; private set; }
        
        internal CustomSidePanel(string name, TranslationReference translation)
        {
            PanelName = name;
            Translation = translation;
        }

        public delegate void SidePanelLoad(Transform panelTransform);

        public event SidePanelLoad OnSidePanelLoaded;

        internal void Preload()
        {
            Preloaded = true;
        }

        internal void OnSidePanelFocus()
        {
            if (Loaded) return;
            OnSidePanelLoaded?.Invoke(PanelContentTransform);
            Loaded = true;
        }
    }
}
