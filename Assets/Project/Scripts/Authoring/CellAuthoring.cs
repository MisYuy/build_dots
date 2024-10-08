using QTS.QWorld.Component;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class CellAuthoring : MonoBehaviour
    {
        public Vector2 coordinate;

        class Baker : Baker<CellAuthoring>
        {
            public override void Bake(CellAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                AddComponent(entity, new CellComponent()
                {
                    coordinate = new int2((int)authoring.coordinate.x, (int)authoring.coordinate.y),
                });

                AddBuffer<CellMember>(entity);
            }
        }
    }
}