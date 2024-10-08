using Unity.Entities;
using Unity.Mathematics;

namespace QTS.QWorld.Component
{
    public struct FrustumCullingTag : IComponentData
    {
        public bool isVisible;
        public int rootMeshId; // 0 - Is not visible
    }

    public struct CameraComponent : IComponentData
    {
        public float4x4 viewProjectionMatrix;
    }
}