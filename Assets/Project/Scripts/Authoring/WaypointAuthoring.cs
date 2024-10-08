using System.Collections.Generic;
using QTS.QWorld.Component;
using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class WaypointAuthoring : MonoBehaviour
    {
        public WaypointAuthoring previousWaypoint;
        public WaypointAuthoring nextWaypoint;

        [Range(0f, 20f)]
        public float width = 1f;

        public List<WaypointAuthoring> branches;

        [Range(0f, 1f)]
        public float branchRatio = 0.5f;

        public TrafficLightAuthoring trafficLightCheck;
        public bool isAvoidWaiting;

        private void OnValidate()
        {
            if (branches == null)
                return;
            foreach (var branch in branches)
            {
                if (!branch.branches.Contains(this))
                    branch.branches.Add(this);
            }
        }

        class Baker : Baker<WaypointAuthoring>
        {
            public override void Bake(WaypointAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                var component = new WaypointComponent
                {
                    rootWaypoint = entity,
                    previousWaypointEntity = authoring.previousWaypoint != null ? GetEntity(authoring.previousWaypoint, TransformUsageFlags.None) : entity,
                    nextWaypointEntity = authoring.nextWaypoint != null ? GetEntity(authoring.nextWaypoint, TransformUsageFlags.None) : entity,
                    width = authoring.width,
                    branchRatio = authoring.branchRatio,
                    position = authoring.transform.position,
                    right = authoring.transform.right,
                    isAvoidWaiting = authoring.isAvoidWaiting,
                };

                if (authoring.trafficLightCheck != null)
                    component.trafficLightCheck = GetEntity(authoring.trafficLightCheck, TransformUsageFlags.None);

                AddComponent(entity, component);

                var branchesBuffer = AddBuffer<WaypointBufferElement>(entity);

                foreach (var waypoint in authoring.branches)
                {
                    branchesBuffer.Add(new WaypointBufferElement { waypoint = GetEntity(waypoint, TransformUsageFlags.None) });
                }
            }
        }
    }
}
