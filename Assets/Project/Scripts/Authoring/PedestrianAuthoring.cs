using QTS.QWorld.Component;
using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class PedestrianAuthoring : MonoBehaviour
    {
        public Vector3 destination;
        Vector3 lastPosition;
        public bool reachedDestination;
        public float stopDistance = 1f;
        public float rotationSpeed = 15f;
        public float minSpeed = 1f, maxSpeed = 3f;
        public float movementSpeed;

        public WaypointAuthoring firstWaypoint;

        class Baker : Baker<PedestrianAuthoring>
        {
            public override void Bake(PedestrianAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PedestrianComponent
                {
                    destination = authoring.destination,
                    lastPosition = authoring.lastPosition,
                    reachedDestination = true,
                    stopDistance = authoring.stopDistance,
                    rotationSpeed = authoring.rotationSpeed,
                    minSpeed = authoring.minSpeed,
                    maxSpeed = authoring.maxSpeed,
                    preWaypoint = GetEntity(authoring.firstWaypoint, TransformUsageFlags.None), // Start with null
                    curWaypoint = GetEntity(authoring.firstWaypoint, TransformUsageFlags.None)  // Start with null
                });

                AddComponent(entity, new FrustumCullingTag
                {
                    isVisible = true,
                });
            }
        }
    }
}
