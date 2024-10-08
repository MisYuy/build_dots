using System;
using System.Collections.Generic;
using QTS.QWorld.Component;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class CarWaypoint : MonoBehaviour
    {
        public List<Branch> branches = new List<Branch>();
        public float width = 3f;
        public bool isRightSide;
        public int arrowCount = 5;
        public float arrowSize = 0.5f;
        public TrafficLightAuthoring trafficLight;
        public bool isShowName = true;

        class Baker : Baker<CarWaypoint>
        {
            public override void Bake(CarWaypoint authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var component = new CarWaypointComponent
                {
                    occupierIndex = -1,
                    pos = authoring.transform.position,
                };

                if (authoring.trafficLight != null)
                    component.trafficLightCheck = GetEntity(authoring.trafficLight, TransformUsageFlags.None);

                AddComponent(entity, component);

                var buffer = AddBuffer<BranchBufferElement>(entity);
                foreach (var branch in authoring.branches)
                {
                    buffer.Add(new BranchBufferElement()
                    {
                        isTurning = branch.isTurning,
                        nextCarWaypoint = GetEntity(branch.carWaypoint, TransformUsageFlags.None),
                        index = branch.index,
                    });
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.5f);

            if (isShowName)
            {
#if UNITY_EDITOR
                Handles.Label(transform.position + Vector3.up * 1f, transform.name, new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.white }
                });
#endif
            }


            foreach (var branch in branches)
            {
                if (branch.carWaypoint != null)
                {
                    Gizmos.color = branch.isTurning ? Color.blue : isRightSide ? Color.green : Color.red;
                    DrawArrowedLine(transform.position, branch.carWaypoint.transform.position);
                }
            }
        }

        private void DrawArrowedLine(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);

            for (int i = 0; i < arrowCount; i++)
            {
                float t = (float)i / (arrowCount - 1);
                Vector3 point = Vector3.Lerp(start, end, t);

                DrawArrow(point, direction);
            }

            Gizmos.DrawLine(start, end);
        }

        private void DrawArrow(Vector3 position, Vector3 direction)
        {
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;

            Vector3 arrowStart = position - direction * arrowSize * 0.5f;
            Vector3 arrowEnd = position + direction * arrowSize * 0.5f;

            Gizmos.DrawLine(arrowStart, arrowEnd);
            Gizmos.DrawLine(arrowEnd, arrowEnd - direction * arrowSize * 0.5f + right * arrowSize * 0.25f);
            Gizmos.DrawLine(arrowEnd, arrowEnd - direction * arrowSize * 0.5f - right * arrowSize * 0.25f);
        }
    }

    [Serializable]
    public class Branch
    {
        public CarWaypoint carWaypoint;
        public bool isTurning;
        public int index = -1;
    }
}