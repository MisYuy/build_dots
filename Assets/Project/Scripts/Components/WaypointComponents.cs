using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QTS.QWorld.Component
{
    public struct WaypointBufferElement : IBufferElementData
    {
        public Entity waypoint;
    }

    public struct WaypointComponent : IComponentData
    {
        public Entity rootWaypoint;
        public Entity previousWaypointEntity;
        public Entity nextWaypointEntity;

        public float width;
        public float branchRatio;

        public float3 position;
        public float3 right;

        public Entity trafficLightCheck;
        [MarshalAs(UnmanagedType.U1)]
        public bool isAvoidWaiting;

        public Vector3 GetPosition(float t)
        {
            float3 minBound = position + right * width / 2f;
            float3 maxBound = position - right * width / 2f;
            return Vector3.Lerp(minBound, maxBound, t);
        }
    }
}
