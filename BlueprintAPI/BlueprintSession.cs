using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using avaness.BlueprintAPI.Network;

namespace avaness.BlueprintAPI
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class BlueprintSession : MySessionComponentBase
    {
        private int runticks;
        private APILogic api;

        public static BlueprintSession Instance; // the only way to access session comp from other classes and the only accepted static field.

        public string OpenScreenName { get; private set; }
        public NetworkManager Network { get; } = new NetworkManager(); 

        public BlueprintSession()
        {
            Instance = this;
        }

        public override void BeforeStart()
        {
            // executed before the world starts updating
            MyAPIGateway.Gui.GuiControlCreated += Gui_GuiControlCreated;
            MyAPIGateway.Gui.GuiControlRemoved += Gui_GuiControlRemoved;
            Network.Init();
        }

        private void Gui_GuiControlCreated(object obj)
        {
            OpenScreenName = obj.GetType().Name;
        }

        private void Gui_GuiControlRemoved(object obj)
        {
            string removedScreen = obj.GetType().Name;
            if (OpenScreenName == removedScreen)
                OpenScreenName = null;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Gui.GuiControlCreated -= Gui_GuiControlCreated;
            MyAPIGateway.Gui.GuiControlRemoved -= Gui_GuiControlRemoved;
            ProjectorControls.Unload();
            Network.Unload();
            api.Unload();
            api = null;
            Instance = null;
        }

        public void GetBlueprint(Action<List<MyObjectBuilder_CubeGrid>> resultCallback, bool connectSubgrids)
        {
            MyObjectBuilder_CubeGrid grid = new MyObjectBuilder_CubeGrid
            {
                CreatePhysics = false,
                Immune = true,
                DestructibleBlocks = false,
                IsPowered = false,
                Editable = false,
                GridSizeEnum = MyCubeSize.Large,
            };
            grid.CubeBlocks.Add(new MyObjectBuilder_Projector
            {
                SubtypeName = ProjectorLogic.ProjectorSubtype,
                Enabled = true,
            });
            MyAPIGateway.Entities.RemapObjectBuilder(grid);
            MyAPIGateway.Entities.CreateFromObjectBuilderParallel(grid, false, (e) => OnProjectorGridReady(e, resultCallback, connectSubgrids));
        }

        private void OnProjectorGridReady(IMyEntity entity, Action<List<MyObjectBuilder_CubeGrid>> onResult, bool connectSubgrids)
        {
            entity.Flags &= ~EntityFlags.Sync; // MyEntity.SyncFlag
            entity.Save = false; // MyEntity.Save
            entity.Synchronized = false; // !MyEntity.IsPreview
            MyAPIGateway.Entities.AddEntity(entity);

            IMyCubeGrid grid = (IMyCubeGrid)entity;
            IMyProjector projector = grid.GetFatBlocks<IMyProjector>().First();
            projector.GameLogic.GetAs<ProjectorLogic>().OpenBlueprintScreen((b, l) =>
            {
                b.Close();
                onResult(l);
            }, connectSubgrids);
        }

        public override void UpdateAfterSimulation()
        {
            runticks++;

            if (api == null && runticks >= 300)
            {
                api = new APILogic();
                RemoveUpdateAfter();
            }
        }

        private void RemoveUpdateAfter()
        {
            MyAPIGateway.Utilities.InvokeOnGameThread(() => UpdateOrder &= ~MyUpdateOrder.AfterSimulation);
        }
    }
}