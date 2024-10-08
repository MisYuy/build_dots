using QTS.QWorld.Component;
using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class LampAuthoring : MonoBehaviour
    {
        class Baker : Baker<LampAuthoring>
        {
            public override void Bake(LampAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                AddComponent(entity, new LampComponent()
                {
                    value = new Unity.Mathematics.float4(0, 0, 0, 1)
                });
            }
        }
    }
}