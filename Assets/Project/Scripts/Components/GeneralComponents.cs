using Unity.Entities;

namespace QTS.QWorld.Component
{
    public struct LastTimeExecuteJob : IComponentData
    {
        public float value;
    }
}