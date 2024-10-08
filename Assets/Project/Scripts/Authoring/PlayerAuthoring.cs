using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new PlayerComponent() { });
            }
        }
    }

    public struct PlayerComponent : IComponentData
    {
        public float3 position;
    }
}