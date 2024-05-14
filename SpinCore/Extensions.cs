namespace SpinCore
{
    public static class Extensions
    {
        public static int ClampInt(this IntRange range, int index)
        {
            if (index >= range.max)
                index = range.max - 1;
            if (index < 0)
                index = 0;
            return index;
        }
    }
}
