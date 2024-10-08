using System;
using Unity.Entities;
using Unity.Mathematics;

namespace QTS.QWorld.Component
{
    public struct CarWaypointComponent : IComponentData
    {
        public int occupierIndex;
        public float3 pos;
        public Entity trafficLightCheck;
    }

    public struct BranchBufferElement : IBufferElementData
    {
        public bool isTurning;
        public Entity nextCarWaypoint;
        public int index;
    }
}