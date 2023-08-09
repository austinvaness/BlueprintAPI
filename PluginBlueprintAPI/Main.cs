using HarmonyLib;
using VRage.Plugins;

namespace avaness.BlueprintAPI.Plugin
{
    public class Main : IPlugin
    {
        public static Main Instance { get; private set; }

        public Harmony Harmony { get; private set; }

        public Main()
        {
            Instance = this;
        }

        public void Dispose()
        {
            Instance = null;
        }

        public void Init(object gameInstance)
        {
            Harmony = new Harmony("avaness.BlueprintAPI.Plugin");
        }

        public void Update()
        {

        }
    }
}
