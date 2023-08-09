using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;

namespace avaness.BlueprintAPI
{
    static class ProjectorControls
    {
        private static bool init = false;
        private static Action<IMyTerminalBlock> openBlueprints;

        public static bool Ready => init;

        public static void Init()
        {
            if (init)
                return;

            init = true;
            List<IMyTerminalControl> projectorControls;
            MyAPIGateway.TerminalControls.GetControls<IMyProjector>(out projectorControls);

            IMyTerminalControlButton openBlueprints = projectorControls.FirstOrDefault(x => x.Id == "Blueprint") as IMyTerminalControlButton;
            if (openBlueprints == null)
            {
                MyLog.Default.WriteLine("BlueprintAPI Error: No blueprint control found! Mod will not function.");
                return;
            }
            ProjectorControls.openBlueprints = openBlueprints.Action;
        }

        public static void Unload()
        {
            openBlueprints = null;
        }

        public static void OpenBlueprints(IMyProjector block)
        {
            openBlueprints?.Invoke(block);
        }
    }
}
