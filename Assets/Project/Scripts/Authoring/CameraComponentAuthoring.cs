using QTS.QWorld.Component;
using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class CameraComponentAuthoring : MonoBehaviour
    {
        class Baker : Baker<CameraComponentAuthoring>
        {
            public override void Bake(CameraComponentAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new CameraComponent());
            }
        }
    }
}