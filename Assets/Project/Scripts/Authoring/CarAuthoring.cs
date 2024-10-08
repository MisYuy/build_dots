using QTS.QWorld.Component;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class CarAuthoring : MonoBehaviour
    {
        public float speed = 7f;
        public CarWaypoint spawnPoint;
        public CarWaypoint targetWaypoint;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Check if spawnPoint is not null
            if (spawnPoint != null && spawnPoint.branches != null && spawnPoint.branches.Count > 0)
            {
                // Randomly select a target waypoint from the branches
                targetWaypoint = spawnPoint.branches[UnityEngine.Random.Range(0, spawnPoint.branches.Count)].carWaypoint;

                // Set the position to the spawn point's position
                transform.position = spawnPoint.transform.position;

                // Ensure the hierarchy window updates to show the selection
                EditorApplication.DirtyHierarchyWindowSorting();

                // This will make sure the inspector updates for the new selection
                EditorUtility.SetDirty(this);
            }
        }
#endif

        class Baker : Baker<CarAuthoring>
        {
            public override void Bake(CarAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new CarComponent
                {
                    speed = authoring.speed,
                    targetPos = authoring.targetWaypoint.transform.position,
                    targetWaypointEntity = GetEntity(authoring.targetWaypoint, TransformUsageFlags.None),
                    indexBranch = -1
                });

                AddComponent(entity, new FrustumCullingTag() { isVisible = true });
            }
        }
    }
}