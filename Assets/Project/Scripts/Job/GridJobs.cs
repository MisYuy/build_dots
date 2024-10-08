using QTS.QWorld.Component;
using QTS.QWorld.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace QTS.QWorld.Job
{
    [BurstCompile]
    public partial struct InitGridJob : IJobEntity
    {
        public NativeKeyValueArrays<int2, Entity> cellEntities;

        public void Execute([EntityIndexInQuery] in int sortKey, in Entity entity, in CellComponent cellComponent)
        {
            cellEntities.Keys[sortKey] = cellComponent.coordinate;
            cellEntities.Values[sortKey] = entity;
        }
    }

    [BurstCompile]
    public partial struct ClearOldDataJob : IJobEntity
    {
        public void Execute(ref DynamicBuffer<CellMember> cellBuffer)
        {
            cellBuffer.Clear();
        }
    }

    [BurstCompile]
    public partial struct UpdatePedestrianInGridJob : IJobEntity
    {
        [ReadOnly] public GridComponent gridComponent;
        [ReadOnly] public NativeKeyValueArrays<int2, Entity> cellEntities;

        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute([EntityIndexInQuery] in int sortKey, in Entity entity, in LocalTransform localTransform, ref PedestrianComponent pedestrianComponent)
        {
            GridUtils.GetCellCoordinates(localTransform.Position, gridComponent.localTransform, gridComponent.size, gridComponent.cellSize, out int2 coor);
            GridUtils.GetCellEntityByCoordinate(coor, cellEntities, out Entity cellEntity);

            if (cellEntity != Entity.Null)
            {
                pedestrianComponent.curCell = cellEntity;

                ecb.AppendToBuffer(sortKey, cellEntity, new CellMember()
                {
                    entity = entity,
                    localPosition = localTransform.Position,
                });
            }
        }
    }

    [BurstCompile]
    public partial struct UpdateCarsInGridJob : IJobEntity
    {
        [ReadOnly] public GridComponent gridComponent;
        [ReadOnly] public NativeKeyValueArrays<int2, Entity> cellEntities;

        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute([EntityIndexInQuery] in int sortKey, in Entity entity, in CarComponent carComponent, in LocalTransform localTransform)
        {
            GridUtils.GetCellCoordinates(localTransform.Position, gridComponent.localTransform, gridComponent.size, gridComponent.cellSize, out int2 coor);
            GridUtils.GetCellEntityByCoordinate(coor, cellEntities, out Entity cellEntity);

            if (cellEntity != Entity.Null)
            {
                ecb.AppendToBuffer(sortKey, cellEntity, new CellMember()
                {
                    entity = entity,
                    localPosition = localTransform.Position,
                });
            }
        }
    }

    [BurstCompile]
    public partial struct CheckObstacleForCarJob : IJobEntity
    {
        [ReadOnly] public GridComponent gridComponent;
        [ReadOnly] public NativeKeyValueArrays<int2, Entity> cellEntities;
        [ReadOnly] public BufferLookup<CellMember> cellBufferLookup;

        public void Execute(ref CarComponent carComponent, in LocalTransform localTransform)
        {
            if (carComponent.curTimeToWaitTrafficLight > 0)
                return;

            GridUtils.GetCellCoordinates(localTransform.Position, gridComponent.localTransform, gridComponent.size, gridComponent.cellSize, out int2 coor);
            GridUtils.GetCellEntityByCoordinate(coor, cellEntities, out Entity cellEntity);

            bool check = false;

            if (cellEntity != Entity.Null)
            {
                var cellBuffer = cellBufferLookup[cellEntity];

                foreach (var buffer in cellBuffer)
                {
                    if (IsAhead(carComponent, localTransform, buffer.localPosition))
                    {
                        check = true;
                        break;
                    }
                }
            }

            carComponent.isWaiting = check;
        }

        public bool IsAhead(CarComponent carComponent, LocalTransform carTransform, float3 posCheck)
        {
            float checkDistance = 7f; // Adjust this value based on your needs
            float checkWidth = 4f; // Adjust this value based on your car's width

            // Calculate forward vector
            float3 forward = math.forward(carTransform.Rotation);

            // Calculate the vector from car to pedestrian
            float3 toPedestrian = posCheck - carTransform.Position;

            // Project this vector onto the forward direction
            float forwardProjection = math.dot(toPedestrian, forward);

            // Check if pedestrian is in front of the car and within the check distance
            if (forwardProjection > 0 && forwardProjection < checkDistance)
            {
                // Calculate the perpendicular distance from the car's forward line
                float3 perpendicularVector = toPedestrian - forward * forwardProjection;
                float perpendicularDistance = math.length(perpendicularVector);

                // Check if the pedestrian is within the width we're checking
                if (perpendicularDistance < checkWidth / 2)
                {
                    return true;
                }
            }

            return false;
        }
    }
}