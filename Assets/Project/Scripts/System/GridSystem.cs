using QTS.QWorld.Component;
using QTS.QWorld.Job;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace QTS.QWorld.System
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(PedestrianSystem))]
    [UpdateBefore(typeof(CarSystem))]
    public partial class GridSystem : SystemBase
    {
        private bool _isInited;

        public NativeKeyValueArrays<int2, Entity> _cellEntities;

        private ComponentLookup<CellComponent> _cellComponentLookup;
        private BufferLookup<CellMember> _cellBufferLookup;

        private EntityCommandBufferSystem _ecbSystem;

        private EntityQuery _query1;
        private EntityQuery _query2;
        private EntityQuery _query3;
        private EntityQuery _query4;
        private EntityQuery _query5;

        private EntityQuery _gridQuery;

        private float _lastTimeExecuteJob;

        private const float PERIOD_TIME_TO_EXECUTE_JOBS = 1f;

        protected override void OnCreate()
        {
            _cellEntities = new NativeKeyValueArrays<int2, Entity>(1000, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            _cellBufferLookup = SystemAPI.GetBufferLookup<CellMember>(false);
            _ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

            _query1 = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<CellComponent>()
                .Build(this);

            _query2 = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<CellMember>()
                .Build(this);

            _query3 = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform, PedestrianComponent>()
                .Build(this);

            _query4 = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform, CarComponent>()
                .Build(this);

            _query5 = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform>()
                .WithAllRW<CarComponent>()
                .Build(this);

            _gridQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<GridComponent>()
                .Build(this);

            RequireForUpdate(_gridQuery);
        }

        protected override void OnDestroy()
        {
            _cellEntities.Dispose();
        }

        protected override void OnUpdate()
        {
            if (!_isInited)
            {
                var initGridJob = new InitGridJob()
                {
                    cellEntities = _cellEntities,
                };
                Dependency = initGridJob.ScheduleParallel(_query1, Dependency);
                _isInited = true;
                return;
            }

            float elapesTime = (float)SystemAPI.Time.ElapsedTime;
            if (elapesTime - _lastTimeExecuteJob < PERIOD_TIME_TO_EXECUTE_JOBS)
                return;

            _lastTimeExecuteJob = elapesTime;

            var gridComponent = _gridQuery.GetSingleton<GridComponent>();
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

            _cellBufferLookup.Update(this);

            var clearOldDataJob = new ClearOldDataJob() { };
            Dependency = clearOldDataJob.ScheduleParallel(_query2, Dependency);

            var updatePedestrianGridJob = new UpdatePedestrianInGridJob()
            {
                ecb = ecb,
                gridComponent = gridComponent,
                cellEntities = _cellEntities,
            };
            Dependency = updatePedestrianGridJob.ScheduleParallel(_query3, Dependency);

            var updateCarsInGridJob = new UpdateCarsInGridJob()
            {
                ecb = ecb,
                gridComponent = gridComponent,
                cellEntities = _cellEntities,
            };
            Dependency = updateCarsInGridJob.ScheduleParallel(_query4, Dependency);

            var checkObstacleForCar = new CheckObstacleForCarJob()
            {
                gridComponent = gridComponent,
                cellEntities = _cellEntities,
                cellBufferLookup = _cellBufferLookup
            };
            Dependency = checkObstacleForCar.ScheduleParallel(_query5, Dependency);

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }

    }
}