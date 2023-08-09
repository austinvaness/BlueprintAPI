using avaness.BlueprintAPI.Subgrids;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace avaness.BlueprintAPI
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Projector), false, ProjectorSubtype)]
    public class ProjectorLogic : MyGameLogicComponent
    {
        public const string ProjectorSubtype = "BlueprintAPI_Projector";
        private const int GetProjectionDelay = 100;

        private IMyProjector block;
        private bool connectSubgrids;
        private Action<IMyProjector, List<MyObjectBuilder_CubeGrid>> onBlueprintsClosed;
        private IMyEntity interactedEntity;
        private bool screenOpen;
        private int projectionSearchDelay;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // the base methods are usually empty, except for OnAddedToContainer()'s, which has some sync stuff making it required to be called.
            base.Init(objectBuilder);

            block = (IMyProjector)Entity;

            if(!ProjectorControls.Ready)
                NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }


        public override void MarkForClose()
        {
            base.MarkForClose();

            onBlueprintsClosed = null;
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            ProjectorControls.Init();
        }

        public void OpenBlueprintScreen(Action<IMyProjector, List<MyObjectBuilder_CubeGrid>> onClose, bool connectSubgrids)
        {

            if (!ProjectorControls.Ready)
            {
                // This might happen if this is the first projector that was ever created in the world
                Utilities.Invoke(() => OpenBlueprintScreen(onClose, connectSubgrids), 10);
                return;
            }

            block.ResourceSink = new InfiniteEnergyResourceSink();
            MyCubeBlock cube = (MyCubeBlock)block;
            cube.UpdateIsWorking();

            if (!block.IsWorking || IsBlueprintScreenOpen())
            {
                onClose(block, null);
                return;
            }

            interactedEntity = MyAPIGateway.Gui.InteractedEntity;
            this.connectSubgrids = connectSubgrids;
            onBlueprintsClosed = onClose;
            screenOpen = true;
            projectionSearchDelay = 0;
            ProjectorControls.OpenBlueprints(block);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateBeforeSimulation()
        {
            if (screenOpen)
            {
                if (IsBlueprintScreenOpen())
                    return;

                screenOpen = false;
            }

            IMyGui gui = MyAPIGateway.Gui;
            if (interactedEntity == null && gui.GetCurrentScreen == MyTerminalPageEnum.ControlPanel && gui.InteractedEntity == null)
                gui.ChangeInteractedEntity(block, false); // The terminal wasnt open before, so we use the block close event to close it (it gets opened by keen)

            float scale = block.GetValueFloat("Scale");
            if (scale != 1)
            {
                block.SetValueFloat("Scale", 1);
                return;
            }

            if(block.ProjectedGrid == null && projectionSearchDelay < GetProjectionDelay)
            {
                projectionSearchDelay++;
                return;
            }


            Utilities.Invoke(() => NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME);

            if (onBlueprintsClosed == null)
                return;
            onBlueprintsClosed.Invoke(block, GetCubeGrid(connectSubgrids));
            onBlueprintsClosed = null;
        }

        private bool IsBlueprintScreenOpen()
        {
            string currentScreen = BlueprintSession.Instance.OpenScreenName;
            return currentScreen == "MyGuiBlueprintScreen" || currentScreen == "MyGuiBlueprintScreen_Reworked";
        }

        private List<MyObjectBuilder_CubeGrid> GetCubeGrid(bool subgridFix)
        {
            if (block.ProjectedGrid == null)
                return null;

            MyObjectBuilder_Projector projector = (MyObjectBuilder_Projector)block.GetObjectBuilderCubeBlock(true);
            if (projector?.ProjectedGrids == null || projector.ProjectedGrids.Count == 0)
                return null;

            List<MyObjectBuilder_CubeGrid> grids = projector.ProjectedGrids;
            if (subgridFix)
            {
                MechanicalSystem system = new MechanicalSystem();
                PrepBlocks(grids, system);
                system.Attach();
            }
            else
            {
                PrepBlocks(grids);
            }
            return grids;
        }

        private void PrepBlocks(IEnumerable<MyObjectBuilder_CubeGrid> grids, MechanicalSystem system = null)
        {
            foreach (MyObjectBuilder_CubeGrid grid in grids)
            {
                GridMechanicalSystem gridSystem = null;
                if(system != null)
                    gridSystem = new GridMechanicalSystem(grid);

                grid.DestructibleBlocks = true;
                grid.CreatePhysics = true;
                grid.IsRespawnGrid = false;
                grid.EnableSmallToLargeConnections = true;

                for (int i = grid.CubeBlocks.Count - 1; i >= 0; i--)
                {
                    MyObjectBuilder_CubeBlock cubeBuilder = grid.CubeBlocks[i];
                    MyCubeBlockDefinition def;
                    if (MyDefinitionManager.Static.TryGetCubeBlockDefinition(cubeBuilder.GetId(), out def))
                        AddToSystem(gridSystem, cubeBuilder, grid, def);
                    else
                        grid.CubeBlocks.RemoveAtFast(i);
                }

                system?.Add(gridSystem);
            }

        }

        private static void AddToSystem(GridMechanicalSystem system, MyObjectBuilder_CubeBlock block, MyObjectBuilder_CubeGrid grid, MyCubeBlockDefinition def)
        {
            var connector = block as MyObjectBuilder_ShipConnector;
            if (connector != null)
            {
                if (connector.ConnectedEntityId == 0)
                    return;

                if (system == null)
                    connector.ConnectedEntityId = 0;
                else if (connector.IsMaster.HasValue && connector.IsMaster.Value)
                    system.Add(new MechanicalBaseBlock(connector, grid, def, MechanicalConnectionType.Connector));
                else
                    system.Add(new MechanicalTopBlock(connector, grid, def, MechanicalConnectionType.Connector));
            }
            else if (block is MyObjectBuilder_Wheel)
            {
                // There is no ideal way to determine real wheel vs wheel block
                if (system != null && (block.SubtypeName.Contains("RealWheel") || (def.Context != null && !def.Context.IsBaseGame)))
                    system.Add(new MechanicalTopBlock(block, grid, def, MechanicalConnectionType.Wheel));
            }
            else
            {
                MechanicalConnectionType temp;
                var topBlock = block as MyObjectBuilder_AttachableTopBlockBase;
                if (topBlock != null)
                {
                    if (topBlock.ParentEntityId != 0)
                    {
                        if (system != null && MechanicalTopBlock.TryGetConnectionType(topBlock, out temp))
                            system.Add(new MechanicalTopBlock(block, grid, def, temp));
                        else
                            topBlock.ParentEntityId = 0;
                    }
                }
                else
                {
                    var baseBlock = block as MyObjectBuilder_MechanicalConnectionBlock;
                    if (baseBlock != null && baseBlock.TopBlockId.HasValue)
                    {
                        if (system != null && baseBlock.TopBlockId.Value != 0 && MechanicalBaseBlock.TryGetConnectionType(baseBlock, out temp))
                        {
                            system.Add(new MechanicalBaseBlock(baseBlock, grid, def, temp));
                        }
                        else
                        {
                            baseBlock.TopBlockId = null;
                            var motor = baseBlock as MyObjectBuilder_MotorBase;
                            if (motor != null)
                                motor.RotorEntityId = null;
                        }
                    }
                }
            }
        }

    }
}