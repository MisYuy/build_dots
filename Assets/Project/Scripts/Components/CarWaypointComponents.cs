using System;
using Unity.Entities;
using Unity.Mathematics;

namespace QTS.QWorld.Component
{
    public struct CarWaypointComponent : IComponentData
    {
        public int occupierIndex; // The index of car is occupy this carWaypoint
        public float3 pos;
        public Entity trafficLightCheck; // Storage the traffic'entity
    }

    public struct BranchBufferElement : IBufferElementData
    {
        public bool isTurning;
        public Entity nextCarWaypoint;
        public int index; // The index of branch use for to identity branches when have multiple branches
    }
}