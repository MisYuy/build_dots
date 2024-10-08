using QTS.QWorld.Component;
using QTS.QWorld.Job;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace QTS.QWorld.System
{
    partial struct TrafficLightAreaSystem : ISystem
    {
        private ComponentLookup<LampComponent> _lampComponentLookup;
        private EntityCommandBuffer.ParallelWriter _ecbParallel;

        private EntityQuery _query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _lampComponentLookup = state.GetComponentLookup<LampComponent>();

            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TrafficLightComponent>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _lampComponentLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            _ecbParallel = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var updateTrafficLightJob = new UpdateTrafficLightJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                lampComponentLookup = _lampComponentLookup,
                ecb = _ecbParallel
            };

            updateTrafficLightJob.ScheduleParallel(_query);
        }
    }
}
