using Unity.Entities;
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
            }
        }
    }
}