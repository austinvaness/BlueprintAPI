using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace avaness.BlueprintAPI.Network
{
    public class BlueprintRequest
    {
        private static int requestId = 0;

        private readonly ulong player;
        private Action<List<MyObjectBuilder_CubeGrid>> resultCallback;
        private readonly BlueprintRequestPacket request;

        public BlueprintRequest(ulong player, Action<List<MyObjectBuilder_CubeGrid>> resultCallback, bool connectSubgrids)
        {
            this.player = player;
            this.resultCallback = resultCallback;
            request = new BlueprintRequestPacket(++requestId, connectSubgrids);
        }

        public void Send()
        {
            NetworkManager net = BlueprintSession.Instance?.Network;
            if (net == null)
                return;

            net.SendTo(player, request);
            net.OnPacketReceived += OnPacketReceived;
        }

        private void OnPacketReceived(ulong sender, BlueprintRequestPacket packet)
        {
            if(sender == player && packet.RequestId == request.RequestId)
            {
                resultCallback(packet.Blueprint);
                resultCallback = null;
                NetworkManager net = BlueprintSession.Instance?.Network;
                if (net != null)
                    net.OnPacketReceived -= OnPacketReceived;
            }
        }
    }
}
