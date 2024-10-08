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
        public CarWaypoint targetCarWaypoint;

#if UNITY_EDITOR
        // This method is used to automatically set the car's position to the spawn point and automatically set the targetCarWaypoint to the next carWaypoint.
        private void OnValidate()
        {
            if (spawnPoint != null && spawnPoint.branches != null && spawnPoint.branches.Count > 0)
            {
                targetCarWaypoint = spawnPoint.branches[UnityEngine.Random.Range(0, spawnPoint.branches.Count)].carWaypoint;
                transform.position = spawnPoint.transform.position;

                EditorApplication.DirtyHierarchyWindowSorting();
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
                    targetPos = authoring.targetCarWaypoint.transform.position,
                    targetWaypointEntity = GetEntity(authoring.targetCarWaypoint, TransformUsageFlags.None),
                    indexBranch = -1
                });

                AddComponent(entity, new FrustumCullingTag() { isVisible = true });
            }
        }
    }
}