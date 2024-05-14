using UnityEngine;
using XDMenuPlay;

namespace SpinCore.UI
{
    public class CustomMultiChoice : CustomUIComponent
    {
        private XDNavigableOptionMultiChoice _multiChoice;

        internal CustomMultiChoice(GameObject multiChoice) : base(multiChoice)
        {
            _multiChoice = multiChoice.GetComponent<XDNavigableOptionMultiChoice>();
        }

        public void SetValueChangedListener(XDNavigableOptionMultiChoice.OnValueChanged valueChanged) => _multiChoice.state.callbacks.onValueChanged = valueChanged;
        public void SetValueRangeRequestedListener(XDNavigableOptionMultiChoice.OnValueRangeRequested valueRangeRequested) => _multiChoice.state.callbacks.onValueRangeRequested = valueRangeRequested;
        public void SetValueTextRequestedListener(XDNavigableOptionMultiChoice.OnValueTextRequested valueTextRequested) => _multiChoice.state.callbacks.onValueTextRequested = valueTextRequested;
        public void SetCurrentValue(int index) => _multiChoice.TargetIndex = _multiChoice.ValueRange.ClampInt(index);
    }
}
