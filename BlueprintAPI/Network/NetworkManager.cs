using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;

namespace avaness.BlueprintAPI.Network
{
    public class NetworkManager
    {
        ushort PacketId => (ushort)(Utilities.MessageId % ushort.MaxValue);

        public event Action<ulong, BlueprintRequestPacket> OnPacketReceived;

        public void Init()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(PacketId, ReceiveMessage);
        }

        public void Unload()
        {
            OnPacketReceived = null;
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(PacketId, ReceiveMessage);
        }

        public void SendTo(ulong playerId, BlueprintRequestPacket packet)
        {
            MyAPIGateway.Multiplayer.SendMessageTo(PacketId, MyAPIGateway.Utilities.SerializeToBinary(packet), playerId);
        }

        public void SendToServer(BlueprintRequestPacket packet)
        {
            MyAPIGateway.Multiplayer.SendMessageToServer(PacketId, MyAPIGateway.Utilities.SerializeToBinary(packet));
        }

        private void ReceiveMessage(ushort packetId, byte[] data, ulong sender, bool fromServer)
        {
            if (!Utilities.IsServer && !fromServer)
                return;

            BlueprintRequestPacket packet;
            if (TryGetPacket(data, out packet))
            {
                if (OnPacketReceived != null)
                    OnPacketReceived.Invoke(sender, packet);
            }
        }

        private bool TryGetPacket(byte[] data, out BlueprintRequestPacket packet)
        {
            try
            {
                packet = MyAPIGateway.Utilities.SerializeFromBinary<BlueprintRequestPacket>(data);
                return true;
            }
            catch { }
            packet = null;
            return false;
        }
    }
}
