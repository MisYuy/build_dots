using QTS.QWorld.Component;
using QTS.QWorld.Job;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace QTS.QWorld.System
{
    [UpdateAfter(typeof(CameraSystem))]
    public partial struct FrustumCullingSystem : ISystem
    {
        private ComponentLookup<FrustumCullingTag> _cullingTagLookup;
        private ComponentLookup<RenderBounds> _renderBoundLookup;

        private EntityQuery _query1;
        private EntityQuery _query2;
        private EntityQuery _query3;
        private EntityQuery _query4;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _cullingTagLookup = state.GetComponentLookup<FrustumCullingTag>();
            _renderBoundLookup = state.GetComponentLookup<RenderBounds>(true);

            _query1 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<FrustumCullingTag>()
                .WithAll<Child, LocalToWorld>()
                .Build(ref state);

            _query2 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<MaterialMeshInfo>()
                .WithAll<Parent>()
                .WithNone<WheelComponent>()
                .Build(ref state);

            _query3 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<MaterialMeshInfo>()
                .WithAll<FrustumCullingTag, CarComponent>()
                .Build(ref state);

            _query4 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<MaterialMeshInfo>()
                .WithAll<Parent, FrustumCullingTag, WheelComponent>()
                .Build(ref state);

            state.RequireForUpdate<CameraComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _cullingTagLookup.Update(ref state);
            _renderBoundLookup.Update(ref state);

            var cameraData = SystemAPI.GetSingleton<CameraComponent>();

            var detectionJob = new DetectionJob
            {
                CameraViewProjection = cameraData.ViewProjectionMatrix,
                renderBoundLookup = _renderBoundLookup
            };
            state.Dependency = detectionJob.ScheduleParallel(_query1, state.Dependency);

            var cullingForPedestrianJob = new CullingForPedestrianJob
            {
                cullingTagLookup = _cullingTagLookup
            };
            state.Dependency = cullingForPedestrianJob.ScheduleParallel(_query2, state.Dependency);

            var cullingForCarJob = new CullingForCarJob() { };
            state.Dependency = cullingForCarJob.ScheduleParallel(_query3, state.Dependency);

            var cullingForCarWheelJob = new CullingForCarWheelJob()
            {
                cullingTagLookup = _cullingTagLookup,
            };
            state.Dependency = cullingForCarWheelJob.ScheduleParallel(_query4, state.Dependency);
        }
    }
}
