using QTS.QWorld.Authoring;
using UnityEditor;
using UnityEngine;

namespace QTS.QWorld.Editor
{
    [InitializeOnLoad()]
    public class WaypointEditor : MonoBehaviour
    {
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        public static void OnDrawSceneGizmo(WaypointAuthoring waypoint, GizmoType gizmoType)
        {
#if UNITY_EDITOR
            Handles.Label(waypoint.transform.position + Vector3.up * 1f, waypoint.transform.name, new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.white }
            });
#endif

            if ((gizmoType & GizmoType.Selected) != 0)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.yellow * 0.5f;
            }

            Gizmos.DrawSphere(waypoint.transform.position, .1f);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(waypoint.transform.position + (waypoint.transform.right * waypoint.width / 2f),
                waypoint.transform.position - (waypoint.transform.right * waypoint.width / 2f));

            if (waypoint.previousWaypoint != null)
            {
                Gizmos.color = Color.red;
                Vector3 offset = waypoint.transform.right * waypoint.width / 2f;
                Vector3 offsetTo = waypoint.previousWaypoint.transform.right * waypoint.previousWaypoint.width / 2f;

                Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.previousWaypoint.transform.position + offsetTo);
            }
            if (waypoint.nextWaypoint != null)
            {
                Gizmos.color = Color.green;
                Vector3 offset = waypoint.transform.right * -waypoint.width / 2f;
                Vector3 offsetTo = waypoint.nextWaypoint.transform.right * -waypoint.nextWaypoint.width / 2f;

                Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.nextWaypoint.transform.position + offsetTo);
            }

            if (waypoint.branches != null)
            {
                foreach (WaypointAuthoring branch in waypoint.branches)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(waypoint.transform.position, branch.transform.position);
                }
            }
        }
    }
}