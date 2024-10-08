using GPUECSAnimationBaker.Engine.AnimatorSystem;
using QTS.QWorld.Component;
using QTS.QWorld.Job.Pedestrian;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace QTS.QWorld.System
{
    [BurstCompile]
    [UpdateAfter(typeof(GridSystem))]
    public partial struct PedestrianSystem : ISystem
    {
        private ComponentLookup<WaypointComponent> _waypointComponentLookup;
        private ComponentLookup<TrafficLightComponent> _trafficLightComponentLookup;
        private ComponentLookup<PedestrianComponent> _pedestrianLookup;

        private BufferLookup<WaypointBufferElement> _waypointBufferLookup;
        private BufferLookup<CellMember> _cellMembersLookup;

        private ComponentTypeHandle<LocalTransform> _localTransformTypeHandle;
        private ComponentTypeHandle<PedestrianComponent> _pedestrianTypehandle;
        private EntityTypeHandle _entityTypeHandle;

        private EntityQuery _query1;
        private EntityQuery _query2;
        private EntityQuery _query3;

        public void OnCreate(ref SystemState state)
        {
            _waypointComponentLookup = state.GetComponentLookup<WaypointComponent>(true);
            _trafficLightComponentLookup = state.GetComponentLookup<TrafficLightComponent>(true);
            _pedestrianLookup = state.GetComponentLookup<PedestrianComponent>(true);

            _waypointBufferLookup = state.GetBufferLookup<WaypointBufferElement>(true);
            _cellMembersLookup = state.GetBufferLookup<CellMember>(true);

            _localTransformTypeHandle = state.GetComponentTypeHandle<LocalTransform>(false);
            _pedestrianTypehandle = state.GetComponentTypeHandle<PedestrianComponent>(false);
            _entityTypeHandle = state.GetEntityTypeHandle();

            _query1 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<PedestrianComponent>()
                .Build(ref state);

            _query2 = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<PedestrianComponent, LocalTransform>()
                .Build(ref state);

            _query3 = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<GpuEcsAnimatorAspect>()
                .WithAllRW<PedestrianComponent>()
                .Build(ref state);

            state.RequireForUpdate<PedestrianComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _waypointComponentLookup.Update(ref state);
            _waypointBufferLookup.Update(ref state);
            _pedestrianLookup.Update(ref state);

            _trafficLightComponentLookup.Update(ref state);
            _cellMembersLookup.Update(ref state);

            _localTransformTypeHandle.Update(ref state);
            _pedestrianTypehandle.Update(ref state);
            _entityTypeHandle.Update(ref state);

            float deltaTime = SystemAPI.Time.DeltaTime;
            Random random = new Random((uint)UnityEngine.Random.Range(1, 9999));

            var updateDestinationJob = new UpdateDestinationForPedestrianJob
            {
                random = random,
                waypointComponentLookup = _waypointComponentLookup,
                waypointBufferLookup = _waypointBufferLookup,
                deltaTime = deltaTime,
                cellMemberLookup = _cellMembersLookup,
                pedestrianTypeHandle = _pedestrianTypehandle,
                entityTypeHandle = _entityTypeHandle
                //pedestrianLookup = _pedestrianLookup
            };
            state.Dependency = updateDestinationJob.ScheduleParallel(_query1, state.Dependency);

            var checkTrafficLightJob = new CheckTrafficLightJob()
            {
                waypointLookup = _waypointComponentLookup,
                trafficLightComponentLookup = _trafficLightComponentLookup
            };
            state.Dependency = checkTrafficLightJob.ScheduleParallel(_query1, state.Dependency);

            var movementJob = new MovementJob
            {
                random = random,
                deltaTime = deltaTime
            };
            state.Dependency = movementJob.ScheduleParallel(_query2, state.Dependency);

            var moveToPartnerJob = new MoveToPartnerJob()
            {
                deltaTime = deltaTime,
                localTransformTypeHandle = _localTransformTypeHandle,
                pedestrianTypeHandle = _pedestrianTypehandle,
                entityTypeHandle = _entityTypeHandle
            };
            state.Dependency = moveToPartnerJob.ScheduleParallel(_query2, state.Dependency);

            var transitionAnimJob = new TransitionAnimationJob();
            state.Dependency = transitionAnimJob.ScheduleParallel(_query3, state.Dependency);

            state.CompleteDependency(); // Chua chac da can thiet
        }
    }
}
