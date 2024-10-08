using Unity.Entities;
using Unity.Mathematics;

namespace QTS.QWorld.Component
{
    public struct FrustumCullingTag : IComponentData
    {
        public bool isVisible;
        public int rootMeshId;
    }

    public struct CameraComponent : IComponentData
    {
        public float4x4 ViewProjectionMatrix;
    }
}