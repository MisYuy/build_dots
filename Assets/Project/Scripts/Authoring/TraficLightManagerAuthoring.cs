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
            }
        }
    }
}
