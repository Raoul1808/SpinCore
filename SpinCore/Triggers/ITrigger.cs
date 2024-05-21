namespace SpinCore.Triggers
{
    /// <summary>
    /// Interface that represents a trigger. You need to implement this for the trigger to be recognized by the system.
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// The timestamp at which the trigger should fire
        /// </summary>
        float Time { get; }
    }
}
