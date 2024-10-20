using XDMenuPlay;

namespace SpinCore.UI
{
    public class CustomTooltip
    {
        private readonly XDTooltipPopoutOpener _tooltipOpener;

        public TranslationReference Tooltip => _tooltipOpener.tooltip;
        public TranslationReference[] TooltipForValue => _tooltipOpener.tooltipForValue;

        internal CustomTooltip(XDTooltipPopoutOpener tooltipOpener)
        {
            _tooltipOpener = tooltipOpener;
        }
    }
}
