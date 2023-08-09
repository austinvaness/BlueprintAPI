using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace avaness.BlueprintAPI.Network
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class BlueprintRequestPacket
    {
        [ProtoMember(1)]
        public int RequestId { get; set; }

        [ProtoMember(2)]
        public bool ConnectSubgrids { get; set; }

        [ProtoMember(3)]
        public List<MyObjectBuilder_CubeGrid> Blueprint { get; set; }

        public BlueprintRequestPacket() { }

        public BlueprintRequestPacket(int requestId, bool connectSubgrids)
        {
            RequestId = requestId;
            ConnectSubgrids = connectSubgrids;
        }
    }
}
