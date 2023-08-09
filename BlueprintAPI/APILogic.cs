using avaness.BlueprintAPI.Network;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game;

namespace avaness.BlueprintAPI
{
    public class APILogic
    {
        public APILogic()
        {
            MyAPIGateway.Utilities.SendModMessage(Utilities.MessageId,
                new MyTuple< Action<Action<List<MyObjectBuilder_CubeGrid>>,bool>, Action<ulong,Action<List<MyObjectBuilder_CubeGrid>>,bool> >
                (GetBlueprint, GetBlueprintServer));
            if(Utilities.IsPlayer)
                BlueprintSession.Instance.Network.OnPacketReceived += Network_OnPacketReceived;
        }

        public void Unload()
        {
            BlueprintSession.Instance.Network.OnPacketReceived -= Network_OnPacketReceived;
        }

        // Important: This method gets patched optionally by the client plugin, so all blueprint requests must pass through here
        public void GetBlueprint(Action<List<MyObjectBuilder_CubeGrid>> resultCallback, bool connectSubgrids)
        {
            if (resultCallback == null)
                return;

            if(Utilities.IsPlayer)
                BlueprintSession.Instance?.GetBlueprint(resultCallback, connectSubgrids);
            else
                resultCallback(null);

        }

        private void GetBlueprintServer(ulong playerId, Action<List<MyObjectBuilder_CubeGrid>> resultCallback, bool connectSubgrids)
        {
            if (resultCallback == null || playerId == 0)
                return;

            if (Utilities.IsServer)
            {
                BlueprintRequest blueprintRequest = new BlueprintRequest(playerId, resultCallback, connectSubgrids);
                blueprintRequest.Send();
            }
            else
            {
                resultCallback(null);
            }
        }

        // Handle requests from the server
        private void Network_OnPacketReceived(ulong sender, BlueprintRequestPacket packet)
        {
            GetBlueprint((l) =>
            {
                packet.Blueprint = l;
                BlueprintSession.Instance?.Network.SendToServer(packet);
            }, 
            packet.ConnectSubgrids);
        }
    }
}
