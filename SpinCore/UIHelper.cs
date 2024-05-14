using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XDMenuPlay;

namespace SpinCore
{
    public static class UIHelper
    {
        internal static GameObject SidePanelButtonBase;
        internal static GameObject MultiChoiceBase;
        
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

        public static GameObject CreateToggle(
            Transform parent,
            string name,
            string translationKey,
            bool defaultValue,
            Action<bool> onValueChange)
        {
            return CreateToggle(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                onValueChange
            );
        }
        
        public static GameObject CreateToggle(
            Transform parent,
            string name,
            TranslationReference translation,
            bool defaultValue,
            Action<bool> onValueChange)
        {
            return CreateMultiChoiceButton(
                parent,
                name,
                translation,
                defaultValue ? 1 : 0,
                v => onValueChange.Invoke(v == 1),
                () => new IntRange(0, 2),
                v => v == 1 ? "UI_Yes" : "UI_No"
            );
        }

        public static GameObject CreateMultiChoiceButton<T>(
            Transform parent,
            string name,
            string translationKey,
            T defaultValue,
            Action<T> onValueChange) where T : Enum
        {
            return CreateMultiChoiceButton(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                onValueChange
            );
        }
        
        public static GameObject CreateMultiChoiceButton<T>(
            Transform parent,
            string name,
            TranslationReference translation,
            T defaultValue,
            Action<T> onValueChange) where T : Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));
            int defaultValueInt = values.ToList().IndexOf(defaultValue);
            return CreateMultiChoiceButton(
                parent,
                name,
                translation,
                defaultValueInt,
                v => onValueChange.Invoke(values[v]),
                () => new IntRange(0, values.Length),
                v => values[v].ToString()
            );
        }

        public static GameObject CreateMultiChoiceButton(
            Transform parent,
            string name,
            string translationKey,
            int defaultValue,
            XDNavigableOptionMultiChoice.OnValueChanged valueChanged,
            XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested,
            XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested)
        {
            return CreateMultiChoiceButton(
                parent,
                name,
                new TranslationReference(translationKey, false),
                defaultValue,
                valueChanged,
                valueRangeRequested,
                valueTextRequested
            );
        }
        
        public static GameObject CreateMultiChoiceButton(
            Transform parent,
            string name,
            TranslationReference translation,
            int defaultValue,
            XDNavigableOptionMultiChoice.OnValueChanged valueChanged,
            XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested,
            XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested)
        {
            var button = GameObject.Instantiate(MultiChoiceBase, parent);
            button.name = name;
            button.SetActive(true);
            button.GetComponentInChildren<TranslatedTextMeshPro>().SetTranslation(translation);
            button.GetComponent<XDNavigableOptionMultiChoice>().state.callbacks = new XDNavigableOptionMultiChoice.Callbacks();
            button.GetComponent<XDNavigableOptionMultiChoice>().SetCallbacksAndValue(defaultValue, valueChanged, valueRangeRequested, valueTextRequested);
            return button;
        }
    }
}
