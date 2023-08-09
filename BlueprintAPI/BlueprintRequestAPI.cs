using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game;

namespace avaness.BlueprintAPI
{
    public class BlueprintRequestAPI
    {
        const long MessageId = 8016952063;

        /// <summary>
        /// True when the API has connected to the BlueprintAPI mod.
        /// </summary>
        public bool Enabled { get; private set; } = false;
        private Action onEnabled;

        /// <summary>
        /// Call Close() when done to unregister message handlers. 
        /// Check Enabled to see if the API is communicating with the BlueprintAPI mod. 
        /// </summary>
        /// <param name="onEnabled">Called once the API has connected to the BlueprintAPI mod.</param>
        public BlueprintRequestAPI(Action onEnabled = null)
        {
            this.onEnabled = onEnabled;
            MyAPIGateway.Utilities.RegisterMessageHandler(MessageId, RecieveData);
        }

        /// <summary>
        /// Call this method to cleanup once you are done with it.
        /// </summary>
        public void Close()
        {
            MyAPIGateway.Utilities.UnregisterMessageHandler(MessageId, RecieveData);
            getBlueprint = null;
            getBlueprintServer = null;
            onEnabled = null;
        }

        private void RecieveData(object obj)
        {
            if (!Enabled && obj is MyTuple< Action<Action<List<MyObjectBuilder_CubeGrid>>,bool>, Action<ulong,Action<List<MyObjectBuilder_CubeGrid>>,bool> >)
            {
                // Initialization
                var funcs = (MyTuple< Action<Action<List<MyObjectBuilder_CubeGrid>>,bool>, Action<ulong,Action<List<MyObjectBuilder_CubeGrid>>,bool> >)obj;
                getBlueprint = funcs.Item1;
                getBlueprintServer = funcs.Item2;
                Enabled = true;
                if (onEnabled != null)
                {
                    onEnabled.Invoke();
                    onEnabled = null;
                }
            }
        }

        private Action<Action<List<MyObjectBuilder_CubeGrid>>, bool> getBlueprint;
        /// <summary>
        /// Request a blueprint from the local player.
        /// </summary>
        /// <param name="resultCallback">Method that gets called after the blueprint has been requested from the player</param>
        /// <param name="connectSubgrids">True if the mod should attempt to connect subgrids of the blueprint together</param>
        public void GetBlueprint(Action<List<MyObjectBuilder_CubeGrid>> resultCallback, bool connectSubgrids = true)
        {
            if (Enabled)
                getBlueprint(resultCallback, connectSubgrids);
            else
                resultCallback(null);
        }

        private Action<ulong, Action<List<MyObjectBuilder_CubeGrid>>, bool> getBlueprintServer;
        /// <summary>
        /// As the server, request a blueprint from a specific player.
        /// </summary>
        /// <param name="playerId">Player id</param>
        /// <param name="resultCallback">Method that gets called after the blueprint has been requested from the player</param>
        /// <param name="connectSubgrids">True if the mod should attempt to connect subgrids of the blueprint together</param>
        public void GetBlueprint(ulong playerId, Action<List<MyObjectBuilder_CubeGrid>> resultCallback, bool connectSubgrids = true)
        {
            if (Enabled && playerId != 0)
                getBlueprintServer(playerId, resultCallback, connectSubgrids);
            else
                resultCallback(null);
        }
    }
}
