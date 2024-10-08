using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace QTS.QWorld.Component
{
    public struct TrafficLightComponent : IComponentData
    {
        public float timeGreen;
        public float timeYellow;
        public float timeRed;

        public Entity greenLamp;
        public Entity yellowLamp;
        public Entity redLamp;

        public float curTimeToTransition;
        public int curState; // 0: Green, 1: Yellow, 2: Red

        public bool IsRed()
        {
            return curState == 2;
        }
    }

    [MaterialProperty("_Color")]
    public struct LampComponent : IComponentData
    {
        public float4 value;
    }
}
