using Sandbox.Game.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;

namespace avaness.BlueprintAPI
{
    public class InfiniteEnergyResourceSink : MyResourceSinkComponent
    {
        private static readonly MyDefinitionId ElectricityId = MyResourceDistributorComponent.ElectricityId;

        public override ListReader<MyDefinitionId> AcceptedResources => new ListReader<MyDefinitionId>(new List<MyDefinitionId>
        {
            ElectricityId,
        });

        public override IMyEntity TemporaryConnectedEntity { get; set; }

        public override string ComponentTypeDebugString => "InfiniteEnergyResourceSink";

        public override float CurrentInputByType(MyDefinitionId resourceTypeId)
        {
            return 0;
        }

        public override bool IsPowerAvailable(MyDefinitionId resourceTypeId, float power)
        {
            return true;
        }

        public override bool IsPoweredByType(MyDefinitionId resourceTypeId)
        {
            return resourceTypeId == ElectricityId;
        }

        public override float MaxRequiredInputByType(MyDefinitionId resourceTypeId)
        {
            return 0;
        }

        public override float RequiredInputByType(MyDefinitionId resourceTypeId)
        {
            return 0;
        }

        public override void SetInputFromDistributor(MyDefinitionId resourceTypeId, float newResourceInput, bool isAdaptible, bool fireEvents = true)
        {
        }

        public override void SetMaxRequiredInputByType(MyDefinitionId resourceTypeId, float newMaxRequiredInput)
        {
        }

        public override void SetRequiredInputByType(MyDefinitionId resourceTypeId, float newRequiredInput)
        {
        }

        public override Func<float> SetRequiredInputFuncByType(MyDefinitionId resourceTypeId, Func<float> newRequiredInputFunc)
        {
            return newRequiredInputFunc;
        }

        public override float SuppliedRatioByType(MyDefinitionId resourceTypeId)
        {
            return 0;
        }
    }
}
