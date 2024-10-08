using Unity.Entities;
using Unity.Mathematics;

namespace QTS.QWorld.Component
{
    public struct CarComponent : IComponentData
    {
        public float speed;
        public float3 targetPos;
        public Entity targetWaypointEntity;
        public bool isReachedDestination;
        public bool isTurning;
        public float3 turnStartPos;
        public float3 turnControlPoint;
        public float turnProgress;
        public bool isWaiting;
        public int indexBranch;
        public bool isCheckedTrafficLight;
        public float curTimeToWaitTrafficLight;
        public float3 forwardPos; // The pos of a point in front of car, use for to check obsticle, etc...
    }

    public struct WheelComponent : IComponentData
    {
    }
}
