using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpinCore
{
    public static class UIHelper
    {
        internal static GameObject SidePanelButtonBase;
        
        public delegate void SidePanelLoaded(Transform parent);

        public static event SidePanelLoaded OnSidePanelLoaded;

        internal static void CallSidePanelEvent(Transform parent)
        {
            OnSidePanelLoaded?.Invoke(parent);
        }

        public static GameObject CreateButton(Transform parent, string name, string translationKey, UnityAction action) => CreateButton(parent, name, new TranslationReference(translationKey, false), action);
        public static GameObject CreateButton(Transform parent, string name, TranslationReference translation, UnityAction action)
        {
            var button = GameObject.Instantiate(SidePanelButtonBase, parent);
            button.name = name;
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            button.GetComponent<XDNavigableButton>().onClick = new Button.ButtonClickedEvent();
            button.GetComponent<XDNavigableButton>().onClick.AddListener(action);
            return button;
        }
    }
}
