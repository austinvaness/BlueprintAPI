using HarmonyLib;
using ParallelTasks;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Gui;
using Sandbox.Game.GUI;
using Sandbox.Game.SessionComponents.Clipboard;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace avaness.BlueprintAPI.Plugin
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class PluginBlueprintAPI : MySessionComponentBase
    {
        const long MessageId = 8016952063;

        private static MyEntityIdRemapHelper remapHelper = new MyEntityIdRemapHelper();

        public override void BeforeStart()
        {
            MyAPIGateway.Utilities.RegisterMessageHandler(MessageId, RecieveData);
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(MessageId, RecieveData);
        }

        private void RecieveData(object obj)
        {
            if (obj is MyTuple< Action<Action<List<MyObjectBuilder_CubeGrid>>,bool>, Action<long,Action<List<MyObjectBuilder_CubeGrid>>,bool> > funcs)
            {
                Main.Instance.Harmony.Patch(funcs.Item1.Method, new HarmonyMethod(typeof(PluginBlueprintAPI), nameof(GetBlueprintPrefix)));
            }
        }

        public static bool GetBlueprintPrefix(Action<List<MyObjectBuilder_CubeGrid>> resultCallback)
        {
            if(MyGuiScreenGamePlay.ActiveGameplayScreen is MyGuiBlueprintScreen_Reworked)
            {
                resultCallback(null);
                return false;
            }

            MyEntity interactedEntity = null;
            if (MyGuiScreenTerminal.IsOpen)
            {
                interactedEntity = MyGuiScreenTerminal.InteractedEntity;
                MyGuiScreenTerminal.Hide();
            }

            MyGridClipboard clipboard = new MyGridClipboard(MyClipboardComponent.ClipboardDefinition.PastingSettings);

            MyBlueprintUtils.OpenBlueprintScreen(clipboard, allowCopyToClipboard: true, MyBlueprintAccessType.PROJECTOR, delegate (MyGuiBlueprintScreen_Reworked bp)
            {
                if (bp == null)
                    return;

                bp.Closed += delegate (MyGuiScreenBase screen, bool isUnloading)
                {
                    if (isUnloading)
                        return;

                    List<MyObjectBuilder_CubeGrid> copiedGrids = clipboard.CopiedGrids;
                    if(copiedGrids == null || copiedGrids.Count == 0)
                    {
                        ReopenTerminal(interactedEntity);
                        resultCallback(null);
                        return;
                    }

                    ProcessGrids(copiedGrids);

                    ReopenTerminal(interactedEntity);

                    resultCallback(copiedGrids);
                };
            });

            return false;
        }

        private static void ProcessGrids(List<MyObjectBuilder_CubeGrid> copiedGrids)
        {
            RemapObjectBuilderCollection(copiedGrids);

            MyPositionAndOrientation? mainGridPosition = copiedGrids[0].PositionAndOrientation;
            foreach (MyObjectBuilder_CubeGrid grid in copiedGrids)
                ProcessGrid(grid, mainGridPosition?.Position);
        }

        private static void ProcessGrid(MyObjectBuilder_CubeGrid gridBuilder, Vector3D? referencePosition)
        {
            gridBuilder.DestructibleBlocks = true;
            gridBuilder.CreatePhysics = true;
            gridBuilder.IsRespawnGrid = false;
            gridBuilder.EnableSmallToLargeConnections = true;

            if (gridBuilder.CubeBlocks == null)
                gridBuilder.CubeBlocks = new List<MyObjectBuilder_CubeBlock>();
            if (gridBuilder.PositionAndOrientation.HasValue && referencePosition.HasValue)
            {
                MyPositionAndOrientation pos = gridBuilder.PositionAndOrientation.Value;
                pos.Position = pos.Position - referencePosition.Value;
                gridBuilder.PositionAndOrientation = pos;

            }
            for (int i = gridBuilder.CubeBlocks.Count - 1; i >= 0; i--)
            {
                MyObjectBuilder_CubeBlock cube = gridBuilder.CubeBlocks[i];
                if(MyDefinitionManager.Static.TryGetCubeBlockDefinition(cube.GetId(), out _))
                {
                    cube.Owner = 0;
                    cube.BuiltBy = 0;
                    cube.ShareMode = MyOwnershipShareModeEnum.None;
                }
                else
                {
                    gridBuilder.CubeBlocks.RemoveAtFast(i);
                }
            }
        }


        private static void ReopenTerminal(MyEntity interactedEntity)
        {
            if (interactedEntity == null)
                return;

            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            if (localCharacter == null)
                return;

            if (!MyGuiScreenTerminal.IsOpen)
                MyGuiScreenTerminal.Show(MyTerminalPageEnum.ControlPanel, localCharacter, interactedEntity);
            if(interactedEntity is MyTerminalBlock terminalBlock && MyGuiScreenTerminal.IsOpen)
                MyGuiScreenTerminal.SwitchToControlPanelBlock(terminalBlock);
        }

        private static void RemapObjectBuilderCollection(IEnumerable<MyObjectBuilder_EntityBase> objectBuilders)
        {
            string[] entityNames = objectBuilders.Select((MyObjectBuilder_EntityBase x) => x.Name).ToArray();
            foreach (MyObjectBuilder_EntityBase objectBuilder in objectBuilders)
            {
                objectBuilder.Remap(remapHelper);
                if (objectBuilder is MyObjectBuilder_CubeGrid grid)
                    RemapGridBlocks(grid, remapHelper);
            }
            remapHelper.Clear();

            int num = 0;
            foreach (MyObjectBuilder_EntityBase objectBuilder in objectBuilders)
            {
                if (!string.IsNullOrEmpty(entityNames[num]) && !MyEntities.EntityNameExists(entityNames[num]))
                {
                    objectBuilder.Name = entityNames[num];
                }
                num++;
            }
        }

        private static void RemapGridBlocks(MyObjectBuilder_CubeGrid grid, IMyRemapHelper remap)
        {
            foreach (MyObjectBuilder_CubeBlock block in grid.CubeBlocks)
            {
                if (block is MyObjectBuilder_AttachableTopBlockBase topBlock && topBlock.ParentEntityId != 0)
                    topBlock.ParentEntityId = remap.RemapEntityId(topBlock.ParentEntityId); // Keen forgot to remap this
            }
        }
    }
}