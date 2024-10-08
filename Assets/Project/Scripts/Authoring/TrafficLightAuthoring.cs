using QTS.QWorld.Component;
using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class TrafficLightAuthoring : MonoBehaviour
    {
        public float timeGreen = 10f;
        public float timeYellow = 2f;
        public float timeRed = 10f;

        public LampAuthoring greenLamp;
        public LampAuthoring yellowLamp;
        public LampAuthoring redLamp;

        public int firstState; // 0: Green, 1: Yellow, 2: Red

        class Baker : Baker<TrafficLightAuthoring>
        {
            public override void Bake(TrafficLightAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                var component = new TrafficLightComponent
                {
                    timeGreen = authoring.timeGreen,
                    timeYellow = authoring.timeYellow,
                    timeRed = authoring.timeRed,
                    greenLamp = GetEntity(authoring.greenLamp, TransformUsageFlags.None),
                    yellowLamp = GetEntity(authoring.yellowLamp, TransformUsageFlags.None),
                    redLamp = GetEntity(authoring.redLamp, TransformUsageFlags.None),
                    curState = authoring.firstState
                };

                switch (authoring.firstState)
                {
                    case 0:
                        component.curTimeToTransition = authoring.timeGreen;
                        break;
                    case 1:
                        component.curTimeToTransition = authoring.timeYellow;
                        break;
                    case 2:
                        component.curTimeToTransition = authoring.timeRed;
                        break;
                }

                AddComponent(entity, component);
            }
        }
    }
}