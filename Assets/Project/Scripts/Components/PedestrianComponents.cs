using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;

namespace QTS.QWorld.Component
{
    public struct PedestrianComponent : IComponentData
    {
        public float3 destination;
        public float3 lastPosition;

        [MarshalAs(UnmanagedType.U1)]
        public bool reachedDestination;
        public float stopDistance;

        public float rotationSpeed;
        public float minSpeed;
        public float maxSpeed;
        public float movementSpeed;

        public Entity preWaypoint;
        public Entity curWaypoint;

        [MarshalAs(UnmanagedType.U1)]
        public bool isWaiting;
        public float waitingTime;

        public int preState;
        public int state; // 0: wait, idle - 1; walk - 2: run - 3,4: talk, laugh, etc...

        [MarshalAs(UnmanagedType.U1)]
        public bool doTransitionAnim;

        [MarshalAs(UnmanagedType.U1)]
        public bool isCheckedTrafficLight;
        public float curTimeToWaitTrafficLight;

        public Entity curCell;

        [MarshalAs(UnmanagedType.U1)]
        public bool isCheckedToGetPartner; // 0: False, 1: True
        public Entity partnerEntity;
        public float curTimeToInteractWithPartner;

        public void SetNewWaypoint(Entity newWaypoint)
        {
            preWaypoint = curWaypoint;
            curWaypoint = newWaypoint;
        }


        public void SetNewState(int newState)
        {
            if (newState == state)
                return;

            preState = state;
            state = newState;
        }
    }
}
