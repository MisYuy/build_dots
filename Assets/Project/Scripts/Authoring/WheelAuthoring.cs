using QTS.QWorld.Component;
using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class WheelAuthoring : MonoBehaviour
    {
        class Baker : Baker<WheelAuthoring>
        {
            public override void Bake(WheelAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new WheelComponent() { });
                AddComponent(entity, new FrustumCullingTag() { isVisible = true });
            }
        }
    }
}