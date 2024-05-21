using BepInEx;
using BepInEx.Logging;

namespace SpinCore.TestMod
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("srxd.raoul1808.spincore", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string Guid = "srxd.raoul1808.spincore.testmod";
        private const string Name = "SpinCore Test Mod";
        private const string Version = "0.1.0";

        private static ManualLogSource _logger;

        private void Awake()
        {
            _logger = Logger;
            LogInfo($"Hello from {Name}!");
        }

        internal static void LogInfo(object msg) => _logger.LogMessage(msg);
    }
}
