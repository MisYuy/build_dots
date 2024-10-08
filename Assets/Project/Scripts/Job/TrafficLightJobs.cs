using QTS.QWorld.Constant;
using QTS.QWorld.Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace QTS.QWorld.Job
{
    [BurstCompile]
    public partial struct UpdateTrafficLightJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public ComponentLookup<LampComponent> lampComponentLookup;

        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(ref TrafficLightComponent trafficLightComponent, [EntityIndexInQuery] int sortKey)
        {
            trafficLightComponent.curTimeToTransition -= deltaTime;

            if (trafficLightComponent.curTimeToTransition <= 0)
            {
                TransitionState(ref trafficLightComponent);

                var greenLampComponent = lampComponentLookup[trafficLightComponent.greenLamp];
                var yellowLampComponent = lampComponentLookup[trafficLightComponent.yellowLamp];
                var redLampComponent = lampComponentLookup[trafficLightComponent.redLamp];

                greenLampComponent.value = trafficLightComponent.curState == 0 ? ColorConstant.GREEN_COLOR : ColorConstant.BLACK_COLOR;
                yellowLampComponent.value = trafficLightComponent.curState == 1 ? ColorConstant.YELLOW_COLOR : ColorConstant.BLACK_COLOR;
                redLampComponent.value = trafficLightComponent.curState == 2 ? ColorConstant.RED_COLOR : ColorConstant.BLACK_COLOR;

                ecb.SetComponent(sortKey, trafficLightComponent.greenLamp, greenLampComponent);
                ecb.SetComponent(sortKey, trafficLightComponent.yellowLamp, yellowLampComponent);
                ecb.SetComponent(sortKey, trafficLightComponent.redLamp, redLampComponent);
            }
        }

        public void TransitionState(ref TrafficLightComponent trafficLightComponent)
        {
            trafficLightComponent.curState = (trafficLightComponent.curState + 1) % 3;

            trafficLightComponent.curTimeToTransition = trafficLightComponent.curState switch
            {
                0 => trafficLightComponent.timeGreen,
                1 => trafficLightComponent.timeYellow,
                _ => trafficLightComponent.timeRed
            };
        }
    }
}