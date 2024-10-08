using QTS.QWorld.Component;
using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.System
{
    public partial class CameraSystem : SystemBase
    {
        private bool isInitPedestrianRenderInfo = false;

        protected override void OnUpdate()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            Entities
                .WithAll<CameraComponent>()
                .ForEach((ref CameraComponent cameraData) =>
                {
                    cameraData.viewProjectionMatrix = mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix;
                }).WithoutBurst().Run();

        }
    }
}