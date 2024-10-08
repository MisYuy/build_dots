using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class TraficLightManagerAuthoring : MonoBehaviour
    {
        class Baker : Baker<TraficLightManagerAuthoring>
        {
            public override void Bake(TraficLightManagerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                var buffer = AddBuffer<ChildTrafficLight>(entity);
                foreach (Transform child in authoring.transform)
                {
                    var childEntity = GetEntity(child.gameObject, TransformUsageFlags.None);
                    buffer.Add(new ChildTrafficLight { Entity = childEntity });
                }
            }
        }
    }

    public struct ChildTrafficLight : IBufferElementData
    {
        public Entity Entity;
    }
}
