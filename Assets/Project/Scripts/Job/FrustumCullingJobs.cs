using QTS.QWorld.Component;
using QTS.QWorld.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace QTS.QWorld.Job
{
    [BurstCompile]
    public partial struct DetectionJob : IJobEntity
    {
        [ReadOnly] public float4x4 CameraViewProjection;
        [ReadOnly] public ComponentLookup<RenderBounds> renderBoundLookup;

        public void Execute(ref FrustumCullingTag tag, in Entity entity, in DynamicBuffer<Child> children, in LocalToWorld transform)
        {
            if (children.Length == 0)
            {
                return;
            }

            var renderBound = renderBoundLookup.GetRefRO(children[0].Value);

            float3 center = transform.Position;
            float3 extents = renderBound.ValueRO.Value.Extents;

            bool isVisible = GeometryUtils.TestPlanesAABB(in CameraViewProjection, in center, in extents);

            if (tag.isVisible != isVisible)
                tag.isVisible = isVisible;
        }
    }

    [BurstCompile]
    public partial struct CullingForPedestrianJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<FrustumCullingTag> cullingTagLookup;

        public void Execute(ref MaterialMeshInfo materialMeshInfo, in Parent parent)
        {
            if (cullingTagLookup.HasComponent(parent.Value))
            {
                var cullingTag = cullingTagLookup.GetRefRO(parent.Value).ValueRO;
                bool isVisible = cullingTag.isVisible;
                if (isVisible && materialMeshInfo.Mesh == 0)
                    materialMeshInfo.Mesh = cullingTag.rootMeshId;
                else if (!isVisible && materialMeshInfo.Mesh != 0)
                    materialMeshInfo.Mesh = 0;
            }
        }
    }

    [BurstCompile]
    public partial struct CullingForCarJob : IJobEntity
    {
        public void Execute(ref MaterialMeshInfo materialMeshInfo, in FrustumCullingTag cullingTag)
        {
            bool isVisible = cullingTag.isVisible;
            if (isVisible && materialMeshInfo.Mesh == 0)
                materialMeshInfo.Mesh = cullingTag.rootMeshId;
            else if (!isVisible && materialMeshInfo.Mesh != 0)
                materialMeshInfo.Mesh = 0;
        }
    }

    [BurstCompile]
    public partial struct CullingForCarWheelJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<FrustumCullingTag> cullingTagLookup;
        public void Execute(ref MaterialMeshInfo materialMeshInfo, in Parent parent, in FrustumCullingTag cullingTag)
        {
            if (cullingTagLookup.HasComponent(parent.Value))
            {
                var parentCullingTag = cullingTagLookup.GetRefRO(parent.Value);
                bool isVisible = parentCullingTag.ValueRO.isVisible;
                if (isVisible && materialMeshInfo.Mesh == 0 && cullingTag.rootMeshId != 0)
                    materialMeshInfo.Mesh = cullingTag.rootMeshId;
                else if (!isVisible && materialMeshInfo.Mesh != 0)
                    materialMeshInfo.Mesh = 0;
            }
        }
    }
}