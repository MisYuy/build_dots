using QTS.QWorld.Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace QTS.QWorld.Job.Car
{
    [BurstCompile]
    public partial struct UpdateDestinationForCarJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<CarWaypointComponent> carWaypointLookup;
        [ReadOnly] public BufferLookup<BranchBufferElement> branchLookup;
        [ReadOnly] public Unity.Mathematics.Random random;
        [ReadOnly] public float deltaTime;

        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(ref CarComponent carComponent, in Entity entity, in LocalTransform localTransform, [EntityIndexInQuery] int sortKey)
        {
            if (carComponent.curTimeToWaitTrafficLight > 0)
            {
                carComponent.curTimeToWaitTrafficLight -= deltaTime;
                return;
            }

            if (math.distance(localTransform.Position, carComponent.targetPos) < 1f)
            {
                carComponent.isReachedDestination = true;
            }

            if (carComponent.isReachedDestination)
            {
                if (branchLookup.HasBuffer(carComponent.targetWaypointEntity))
                {
                    var buffer = branchLookup[carComponent.targetWaypointEntity];
                    BranchBufferElement branch = new BranchBufferElement();

                    int curIndexBranch = carComponent.indexBranch;
                    if (curIndexBranch != -1 && buffer.Length > 1)
                    {
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            if (curIndexBranch == buffer[i].index)
                            {
                                branch = buffer[i];
                                break;
                            }
                        }
                    }
                    else
                    {
                        int indexRandom = random.NextInt(0, buffer.Length);
                        branch = buffer[indexRandom];
                    }

                    var newWaypoint = branch.nextCarWaypoint;

                    if (carWaypointLookup.HasComponent(newWaypoint))
                    {
                        var waypointComponent = carWaypointLookup[newWaypoint];
                        if (waypointComponent.occupierIndex == -1)
                        {
                            carComponent.isWaiting = false;

                            waypointComponent.occupierIndex = entity.Index;

                            // Use EntityCommandBuffer to defer the change
                            ecb.SetComponent(sortKey, newWaypoint, waypointComponent);

                            var oldWaypoint = carComponent.targetWaypointEntity;
                            var oldWaypointComponent = carWaypointLookup[oldWaypoint];
                            oldWaypointComponent.occupierIndex = -1;

                            ecb.SetComponent(sortKey, oldWaypoint, oldWaypointComponent);

                            carComponent.targetPos = waypointComponent.pos;
                            carComponent.targetWaypointEntity = newWaypoint;

                            carComponent.indexBranch = branch.index;
                        }
                        else
                        {
                            carComponent.isWaiting = true;
                        }
                    }

                    carComponent.isTurning = false;

                    if (branch.isTurning)
                    {
                        carComponent.isTurning = true;

                        carComponent.turnStartPos = localTransform.Position;
                        carComponent.turnProgress = 0f;

                        // Tính điểm kiểm soát cho đường cong Bezier
                        float3 midPoint = (localTransform.Position + carComponent.targetPos) * 0.5f;
                        float3 direction = math.normalize(carComponent.targetPos - localTransform.Position);
                        float3 perpendicular = new float3(-direction.z, direction.y, direction.x);
                        float curveStrength = math.distance(localTransform.Position, carComponent.targetPos) * 0.5f;
                        carComponent.turnControlPoint = midPoint + perpendicular * curveStrength;
                    }

                    carComponent.isReachedDestination = false;
                    carComponent.isCheckedTrafficLight = false;
                }
            }
        }
    }

    [BurstCompile]
    public partial struct CheckTrafficLightJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<TrafficLightComponent> trafficLightComponentLookup;
        [ReadOnly] public ComponentLookup<CarWaypointComponent> carWaypointLookup;

        public void Execute(ref CarComponent carComponent)
        {
            if (carComponent.isCheckedTrafficLight)
                return;

            var trafficLightComponent = carWaypointLookup[carComponent.targetWaypointEntity];
            var trafficLightCheck = trafficLightComponent.trafficLightCheck;
            if (trafficLightCheck != Entity.Null)
            {
                var trafficLightCheckComponent = trafficLightComponentLookup[trafficLightCheck];
                if (trafficLightCheckComponent.IsRed())
                    carComponent.curTimeToWaitTrafficLight = trafficLightCheckComponent.curTimeToTransition;
            }
            carComponent.isCheckedTrafficLight = true;
        }
    }

    [BurstCompile]
    public partial struct MovementJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;

        public void Execute(ref LocalTransform localTransform, ref CarComponent carComponent)
        {
            if (carComponent.isWaiting)
                return;

            float3 newPosition;
            float3 movementDirection;

            if (carComponent.isTurning)
            {
                // Bezier curve calculation (unchanged)
                float t = carComponent.turnProgress;
                float3 p0 = carComponent.turnStartPos;
                float3 p1 = carComponent.turnControlPoint;
                float3 p2 = carComponent.targetPos;

                newPosition = math.pow(1 - t, 2) * p0 +
                              2 * (1 - t) * t * p1 +
                              math.pow(t, 2) * p2;

                movementDirection = 2 * (1 - t) * (p1 - p0) + 2 * t * (p2 - p1);

                carComponent.turnProgress += carComponent.speed * deltaTime / math.distance(p0, p2);

                if (carComponent.turnProgress >= 1f)
                {
                    carComponent.isTurning = false;
                    carComponent.turnProgress = 0f;
                    newPosition = carComponent.targetPos;
                    movementDirection = carComponent.targetPos - localTransform.Position;
                }
            }
            else
            {
                // Straight movement logic (unchanged)
                movementDirection = carComponent.targetPos - localTransform.Position;
                float distance = math.length(movementDirection);

                if (distance > math.EPSILON)
                {
                    float3 movement = math.normalize(movementDirection) * carComponent.speed * deltaTime;
                    newPosition = math.length(movement) > distance ? carComponent.targetPos : localTransform.Position + movement;
                }
                else
                {
                    newPosition = localTransform.Position;
                }
            }

            // Update position
            localTransform.Position = newPosition;

            // Calculate rotation
            if (math.lengthsq(movementDirection) > math.EPSILON)
            {
                quaternion targetRotation = quaternion.LookRotation(movementDirection, new float3(0, 1, 0));

                // Smooth rotation using slerp
                float rotationSpeed = 5f; // Adjust this value to change how quickly the car rotates
                quaternion newRotation = math.slerp(localTransform.Rotation, targetRotation, rotationSpeed * deltaTime);

                // Apply new rotation
                localTransform.Rotation = newRotation;
            }

            float3 forwardPosition = localTransform.Position + math.forward(localTransform.Rotation) * 4f;
            carComponent.forwardPos = forwardPosition;
        }
    }

    [BurstCompile]
    public partial struct WheelRotateJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<CarComponent> carComponentLookup;
        [ReadOnly] public float deltaTime;

        public void Execute(ref LocalTransform localTransform, in Parent parent, in WheelComponent wheelComponent)
        {
            var carComponent = carComponentLookup.GetRefRO(parent.Value).ValueRO;
            if (!carComponent.isWaiting && carComponent.curTimeToWaitTrafficLight <= 0)
            {
                localTransform = localTransform.RotateX(carComponent.speed * deltaTime);
            }
        }
    }

    [BurstCompile]
    public partial struct InitCarInfoJob : IJobEntity
    {
        public void Execute(ref FrustumCullingTag cullingTag, in MaterialMeshInfo materialMeshInfo)
        {
            if (materialMeshInfo.Mesh != 0)
                cullingTag.rootMeshId = materialMeshInfo.Mesh;
        }
    }
}