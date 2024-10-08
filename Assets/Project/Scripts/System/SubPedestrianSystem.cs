using QTS.QWorld.Component;
using QTS.QWorld.Job.Pedestrian;
using Unity.Entities;
using Unity.Rendering;

namespace QTS.QWorld.System
{
    [UpdateBefore(typeof(FrustumCullingSystem))]
    public partial class SubPedestrianSystem : SystemBase
    {
        private bool _isInitPedestrianRenderInfo = false;

        private ComponentLookup<MaterialMeshInfo> _materialMeshInfoLookup;

        protected override void OnCreate()
        {
            _materialMeshInfoLookup = GetComponentLookup<MaterialMeshInfo>(true);

            RequireForUpdate<FrustumCullingTag>();
            RequireForUpdate<PedestrianComponent>();
        }

        protected override void OnUpdate()
        {
            if (!_isInitPedestrianRenderInfo)
            {
                _materialMeshInfoLookup.Update(this);

                Dependency = new InitPedestrianInfoJob()
                {
                    materialMeshInfoLookup = _materialMeshInfoLookup,
                }.ScheduleParallel(Dependency);

                _isInitPedestrianRenderInfo = true;

                Dependency.Complete();
            }
        }
    }
}