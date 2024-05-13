using BepInEx;

namespace SpinCore
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        private const string Guid = "srxd.raoul1808.spincore";
        private const string Name = "SpinCore";
        private const string Version = "0.1.0";

        private void Awake()
        {
            Logger.LogMessage($"Hello from {Name}");
        }
    }
}
