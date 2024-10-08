using System.Diagnostics;
using GPUECSAnimationBaker.Engine.AnimatorSystem;
using QTS.QWorld.Component;
using QTS.QWorld.Utility;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditorInternal;

namespace QTS.QWorld.Job.Pedestrian
{

    [BurstCompile]
    public partial struct UpdateDestinationForPedestrianJob : IJobChunk
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public Unity.Mathematics.Random random;

        [ReadOnly] public ComponentLookup<WaypointComponent> waypointComponentLookup;

        [ReadOnly] public BufferLookup<WaypointBufferElement> waypointBufferLookup;
        [ReadOnly] public BufferLookup<CellMember> cellMemberLookup;

        public ComponentTypeHandle<PedestrianComponent> pedestrianTypeHandle;
        public EntityTypeHandle entityTypeHandle;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<PedestrianComponent> pedestrianComponents = chunk.GetNativeArray(ref pedestrianTypeHandle);
            NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);

            for (int i = 0; i < chunk.Count; i++)
            {
                var pedestrianComponent = pedestrianComponents[i];
                var entity = entities[i];

                if (pedestrianComponent.curTimeToWaitTrafficLight > 0)
                {
                    pedestrianComponent.curTimeToWaitTrafficLight -= deltaTime;
                    return;
                }

                if (pedestrianComponent.reachedDestination)
                {
                    if (pedestrianComponent.isCheckedToGetPartner == 0)
                    {
                        pedestrianComponent.isCheckedToGetPartner = 1;
                        float randomValue = random.NextFloat(0, 1f);
                        if (randomValue < 0.1f && pedestrianComponent.curCell != Entity.Null)
                        {
                            // To get partner
                            var cellMembers = cellMemberLookup[pedestrianComponent.curCell];
                            if (cellMembers.Length > 1)
                            {
                                foreach (var cellMember in cellMembers)
                                {
                                    if (cellMember.entity != entity)
                                    {
                                        int indexOfPartner = entities.IndexOf(cellMember.entity);

                                        if (indexOfPartner == -1)
                                        {
                                            break;
                                        }

                                        var partnerPedestrianComponent = pedestrianComponents[indexOfPartner];
                                        if (partnerPedestrianComponent.partnerEntity == Entity.Null)
                                        {
                                            pedestrianComponent.partnerEntity = cellMember.entity;
                                            partnerPedestrianComponent.partnerEntity = entity;
                                            pedestrianComponents[indexOfPartner] = partnerPedestrianComponent;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    bool shouldBranch = false;
                    var curWaypointComponent = waypointComponentLookup.GetRefRO(pedestrianComponent.curWaypoint).ValueRO;
                    if (waypointBufferLookup.TryGetBuffer(pedestrianComponent.curWaypoint, out var buffer))
                    {
                        if (buffer.Length > 0)
                        {
                            shouldBranch = random.NextFloat(0f, 1f) < curWaypointComponent.branchRatio;
                        }

                        if (buffer.Length == 1)
                            if (buffer[0].waypoint.Index == pedestrianComponent.preWaypoint.Index)
                                shouldBranch = false;

                        if (shouldBranch)
                        {
                            var newWaypoint = buffer[random.NextInt(0, buffer.Length)].waypoint;

                            if (newWaypoint.Index == pedestrianComponent.preWaypoint.Index && buffer.Length > 1)
                            {
                                while (newWaypoint.Index == pedestrianComponent.preWaypoint.Index)
                                {
                                    newWaypoint = buffer[random.NextInt(0, buffer.Length)].waypoint;
                                }
                                pedestrianComponent.SetNewWaypoint(newWaypoint);
                            }
                            else
                            {
                                pedestrianComponent.SetNewWaypoint(newWaypoint);
                            }
                        }
                        else
                        {
                            GetNextWaypoint(ref pedestrianComponent, curWaypointComponent);
                        }
                        pedestrianComponent.destination = waypointComponentLookup.GetRefRO(pedestrianComponent.curWaypoint).ValueRO.GetPosition(random.NextFloat(0f, 1f));

                        if (!pedestrianComponent.isWaiting)
                        {
                            int randomState = waypointComponentLookup.GetRefRO(pedestrianComponent.curWaypoint).ValueRO.isAvoidWaiting ? random.NextInt(1, 3) : random.NextInt(0, 3);

                            if (randomState == 0)
                            {
                                pedestrianComponent.isWaiting = true;
                                pedestrianComponent.waitingTime = 4f;
                            }

                            if (pedestrianComponent.state != randomState)
                            {
                                pedestrianComponent.SetNewState(randomState);
                                pedestrianComponent.doTransitionAnim = true;
                            }
                        }
                        pedestrianComponent.reachedDestination = false;
                        pedestrianComponent.isCheckedTrafficLight = false;
                        pedestrianComponent.isCheckedToGetPartner = 0;
                    }
                }

                pedestrianComponents[i] = pedestrianComponent;
            }
        }

        public void GetNextWaypoint(ref PedestrianComponent pedestrianComponent, in WaypointComponent curWaypointComponent)
        {
            if (curWaypointComponent.nextWaypointEntity != Entity.Null && curWaypointComponent.nextWaypointEntity.Index != curWaypointComponent.rootWaypoint.Index)
            {
                pedestrianComponent.SetNewWaypoint(curWaypointComponent.nextWaypointEntity);
            }
            else if (curWaypointComponent.previousWaypointEntity.Index != curWaypointComponent.rootWaypoint.Index)
            {
                pedestrianComponent.SetNewWaypoint(curWaypointComponent.previousWaypointEntity);
            }
        }

    }

    [BurstCompile]
    public partial struct CheckTrafficLightJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TrafficLightComponent> trafficLightComponentLookup;
        [ReadOnly] public ComponentLookup<WaypointComponent> waypointLookup;

        public void Execute(ref PedestrianComponent pedestrianComponent)
        {
            if (pedestrianComponent.isCheckedTrafficLight)
                return;

            var trafficLightComponent = waypointLookup[pedestrianComponent.curWaypoint];
            var trafficLightCheck = trafficLightComponent.trafficLightCheck;
            if (trafficLightCheck != Entity.Null)
            {
                var trafficLightCheckComponent = trafficLightComponentLookup[trafficLightCheck];
                if (!trafficLightCheckComponent.IsRed())
                    pedestrianComponent.curTimeToWaitTrafficLight = trafficLightCheckComponent.curTimeToTransition;
            }

            pedestrianComponent.isCheckedTrafficLight = true;
        }
    }

    [BurstCompile]
    public partial struct MovementJob : IJobEntity
    {
        [ReadOnly] public Unity.Mathematics.Random random;
        [ReadOnly] public float deltaTime;

        void Execute(ref LocalTransform localTransform, ref PedestrianComponent pedestrianComponent)
        {
            if (pedestrianComponent.partnerEntity != Entity.Null)
                return;

            if (pedestrianComponent.curTimeToWaitTrafficLight > 0)
                return;

            if (pedestrianComponent.isWaiting)
            {
                pedestrianComponent.waitingTime -= deltaTime;

                if (pedestrianComponent.waitingTime > 0)
                    return;

                if (pedestrianComponent.waitingTime <= 0)
                {
                    pedestrianComponent.isWaiting = false;
                    int randomState = random.NextInt(0, 3);

                    if (randomState == 0)
                    {
                        pedestrianComponent.isWaiting = true;
                        pedestrianComponent.waitingTime = 4f;
                    }

                    if (pedestrianComponent.state != randomState)
                    {
                        pedestrianComponent.SetNewState(randomState);
                        pedestrianComponent.doTransitionAnim = true;
                    }
                }
            }


            if (!math.all(localTransform.Position == pedestrianComponent.destination))
            {
                float3 direction = pedestrianComponent.destination - localTransform.Position;
                direction.y = 0;

                float distance = math.length(direction);

                if (distance >= pedestrianComponent.stopDistance)
                {
                    pedestrianComponent.reachedDestination = false;
                    quaternion targetRotation = quaternion.LookRotationSafe(direction, math.up());
                    localTransform.Rotation = math.slerp(localTransform.Rotation, targetRotation, pedestrianComponent.rotationSpeed * deltaTime);
                    localTransform.Position += math.forward(localTransform.Rotation) * pedestrianComponent.movementSpeed * deltaTime;
                }
                else
                {
                    pedestrianComponent.reachedDestination = true;
                }
            }
        }
    }

    [BurstCompile]
    public partial struct TransitionAnimationJob : IJobEntity
    {
        [ReadOnly] public Random random;

        public void Execute(GpuEcsAnimatorAspect gpuEcsAnimatorAspect, ref PedestrianComponent pedestrianComponent)
        {
            if (pedestrianComponent.doTransitionAnim)
            {
                int state = pedestrianComponent.state;
                switch (state)
                {
                    case 1: // walk
                        gpuEcsAnimatorAspect.RunAnimation(1, transitionSpeed: pedestrianComponent.preState == 0 ? 0 : 1f);
                        pedestrianComponent.movementSpeed = random.NextFloat(pedestrianComponent.minSpeed, pedestrianComponent.maxSpeed / 2f);
                        break;
                    case 2: // run
                        gpuEcsAnimatorAspect.RunAnimation(2, transitionSpeed: pedestrianComponent.preState == 0 ? 0 : 1f);
                        pedestrianComponent.movementSpeed = random.NextFloat(pedestrianComponent.maxSpeed / 2f, pedestrianComponent.maxSpeed);
                        break;
                    default:
                        gpuEcsAnimatorAspect.RunAnimation(state, transitionSpeed: 1f);
                        break;
                }

                pedestrianComponent.doTransitionAnim = false;
                pedestrianComponent.isCheckedTrafficLight = false;
            }
        }
    }

    [BurstCompile]
    public partial struct InitPedestrianInfoJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<MaterialMeshInfo> materialMeshInfoLookup;

        public void Execute(ref FrustumCullingTag frustumCullingTag, in PedestrianComponent pedestrianComponent, in DynamicBuffer<Child> children)
        {
            var materialMeshInfo = materialMeshInfoLookup.GetRefRO(children[0].Value).ValueRO;
            frustumCullingTag.rootMeshId = materialMeshInfo.Mesh;
        }
    }

    [BurstCompile]
    public partial struct MoveToPartnerJob : IJobChunk
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public Random random;

        public ComponentTypeHandle<LocalTransform> localTransformTypeHandle;
        public ComponentTypeHandle<PedestrianComponent> pedestrianTypeHandle;
        public EntityTypeHandle entityTypeHandle;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<LocalTransform> localTransforms = chunk.GetNativeArray(ref localTransformTypeHandle);
            NativeArray<PedestrianComponent> pedestrianComponents = chunk.GetNativeArray(ref pedestrianTypeHandle);
            NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);

            for (int i = 0; i < chunk.Count; i++)
            {
                var pedestrianComponent = pedestrianComponents[i];
                if (pedestrianComponent.partnerEntity == Entity.Null)
                    continue;

                if (pedestrianComponent.curTimeToInteractWithPartner > 0)
                {
                    pedestrianComponent.curTimeToInteractWithPartner -= deltaTime;
                    if (pedestrianComponent.curTimeToInteractWithPartner <= 0)
                    {
                        pedestrianComponent.partnerEntity = Entity.Null;
                        pedestrianComponent.SetNewState(random.NextInt(1, 3));
                        pedestrianComponent.doTransitionAnim = true;
                    }

                    pedestrianComponents[i] = pedestrianComponent;
                    continue;
                }

                int index = entities.IndexOf(pedestrianComponent.partnerEntity);
                if (index == -1)
                    continue;

                var partnerLocalTransform = localTransforms[index];
                float distance = math.distance(partnerLocalTransform.Position, localTransforms[i].Position);

                if (distance <= 1f)
                {
                    pedestrianComponent.curTimeToInteractWithPartner = 15f;
                    pedestrianComponent.SetNewState(random.NextInt(3, 5));
                    pedestrianComponent.doTransitionAnim = true;
                }
                else
                {
                    var localTransform = localTransforms[i];

                    // Calculate direction towards the partner, not the destination
                    float3 direction = partnerLocalTransform.Position - localTransform.Position;
                    direction.y = 0; // Keep the rotation on the horizontal plane
                    quaternion targetRotation = quaternion.LookRotationSafe(direction, math.up());

                    // Rotate towards the partner
                    localTransform.Rotation = math.slerp(localTransform.Rotation, targetRotation, pedestrianComponent.rotationSpeed * deltaTime);

                    // Move towards the partner
                    localTransform.Position += math.forward(localTransform.Rotation) * pedestrianComponent.movementSpeed * deltaTime;

                    localTransforms[i] = localTransform;
                }

                pedestrianComponents[i] = pedestrianComponent;
            }
        }
    }
}