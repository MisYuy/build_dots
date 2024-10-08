using QTS.QWorld.Component;
using QTS.QWorld.Job.Car;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace QTS.QWorld.System
{
    [BurstCompile]
    [UpdateAfter(typeof(GridSystem))]
    public partial struct CarSystem : ISystem
    {
        private ComponentLookup<CarWaypointComponent> _carWaypointLookup;
        private ComponentLookup<TrafficLightComponent> _trafficLightComponentLookup;
        private ComponentLookup<CarComponent> _carComponentLookup;

        private BufferLookup<BranchBufferElement> _branchLookup;

        private EntityCommandBuffer.ParallelWriter _ecbParallel;

        private EntityQuery _query1;
        private EntityQuery _query2;
        private EntityQuery _query3;
        private EntityQuery _query4;

        [BurstCompile]
        void OnCreate(ref SystemState state)
        {
            _carWaypointLookup = state.GetComponentLookup<CarWaypointComponent>(true);
            _trafficLightComponentLookup = state.GetComponentLookup<TrafficLightComponent>(true);
            _carComponentLookup = state.GetComponentLookup<CarComponent>(true);

            _branchLookup = state.GetBufferLookup<BranchBufferElement>(true);

            _query1 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<CarComponent>()
                .WithAll<LocalTransform>()
                .Build(ref state);

            _query2 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<CarComponent>()
                .Build(ref state);

            _query3 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<CarComponent>()
                .WithAllRW<LocalTransform>()
                .Build(ref state);

            _query4 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<LocalTransform>()
                .WithAll<WheelComponent, Parent>()
                .Build(ref state);

            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _carWaypointLookup.Update(ref state);
            _trafficLightComponentLookup.Update(ref state);
            _branchLookup.Update(ref state);
            _carComponentLookup.Update(ref state);

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            _ecbParallel = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var updateDestinationJob = new UpdateDestinationForCarJob()
            {
                carWaypointLookup = _carWaypointLookup,
                branchLookup = _branchLookup,
                ecb = _ecbParallel,
                deltaTime = SystemAPI.Time.DeltaTime,
                random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 999999))
            };
            state.Dependency = updateDestinationJob.ScheduleParallel(_query1, state.Dependency);

            var checkTrafficLightJob = new CheckTrafficLightJob()
            {
                trafficLightComponentLookup = _trafficLightComponentLookup,
                carWaypointLookup = _carWaypointLookup
            };
            state.Dependency = checkTrafficLightJob.ScheduleParallel(_query2, state.Dependency);

            var movementJob = new MovementJob()
            {
                deltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = movementJob.ScheduleParallel(_query3, state.Dependency);

            var wheelRotateJob = new WheelRotateJob()
            {
                carComponentLookup = _carComponentLookup,
                deltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = wheelRotateJob.ScheduleParallel(_query4, state.Dependency);
        }
    }
}