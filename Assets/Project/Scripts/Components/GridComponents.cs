using Unity.Entities;
using Unity.Mathematics;

namespace QTS.QWorld.Component
{
    public struct GridComponent : IComponentData
    {
        public float2 size;
        public float cellSize;
        public float3 localTransform;
    };

    public struct CellComponent : IComponentData
    {
        public int2 coordinate;
    }

    public struct CellMember : IBufferElementData
    {
        public Entity entity;
        public float3 localPosition;
    }
}