using QTS.QWorld.Component;
using Unity.Entities;
using Unity.Rendering;

namespace QTS.QWorld.System
{
    [UpdateBefore(typeof(FrustumCullingSystem))]
    public partial class SubCarSystem : SystemBase
    {
        private bool _isInitInfor = false;

        protected override void OnCreate()
        {
            RequireForUpdate<FrustumCullingTag>();
            RequireForUpdate<CarComponent>();
            RequireForUpdate<WheelComponent>();
        }

        protected override void OnUpdate()
        {
            if (!_isInitInfor)
            {
                Entities
               .ForEach((ref FrustumCullingTag cullingTag, in MaterialMeshInfo info) =>
               {
                   cullingTag.rootMeshId = info.Mesh;
               }).WithoutBurst().Run();
                _isInitInfor = true;
            }
        }
    }
}