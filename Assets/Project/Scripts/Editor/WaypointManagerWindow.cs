using QTS.QWorld.Authoring;
using UnityEditor;
using UnityEngine;

namespace QTS.QWorld.Editor
{
    public class WaypointManagerWindow : EditorWindow
    {
        [MenuItem("Tools/Waypoint Editor")]
        public static void Open()
        {
            GetWindow<WaypointManagerWindow>();
        }

        public Transform waypointRoot;

        private void OnGUI()
        {
            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.PropertyField(obj.FindProperty("waypointRoot"));

            if (waypointRoot == null)
            {
                EditorGUILayout.HelpBox("Root transform must be selected. Please assign a root transform.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.BeginVertical("box");
                DrawButtons();
                EditorGUILayout.EndVertical();
            }

            obj.ApplyModifiedProperties();
        }

        void DrawButtons()
        {
            if (GUILayout.Button("Create Waypoint"))
            {
                CreateWaypoint();
            }

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<WaypointAuthoring>())
            {
                if (GUILayout.Button("Add Branch"))
                {
                    CreateBranch();
                }
                if (GUILayout.Button("Create Blank Waypoint"))
                {
                    CreateBlankWaypoint();
                }
                if (GUILayout.Button("Create Waypoint Before"))
                {
                    CreateWaypointBefore();
                }
                if (GUILayout.Button("Create Waypoint After"))
                {
                    CreateWaypointAfter();
                }
                if (GUILayout.Button("Remove Waypoint"))
                {
                    RemoveWaypoint();
                }
            }
        }

        void CreateBlankWaypoint()
        {
            GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(WaypointAuthoring));
            waypointObject.transform.SetParent(waypointRoot, false);

            Selection.activeGameObject = waypointObject;
        }

        void CreateWaypoint()
        {
            GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(WaypointAuthoring));
            waypointObject.transform.SetParent(waypointRoot, false);

            WaypointAuthoring waypoint = waypointObject.GetComponent<WaypointAuthoring>();
            if (waypointRoot.childCount > 1)
            {
                waypoint.previousWaypoint = waypointRoot.GetChild(waypointRoot.childCount - 2).GetComponent<WaypointAuthoring>();
                waypoint.previousWaypoint.nextWaypoint = waypoint;
                //Place the waypoint at the last position
                waypoint.transform.position = waypoint.previousWaypoint.transform.position;
                waypoint.transform.forward = waypoint.previousWaypoint.transform.forward;
            }

            Selection.activeGameObject = waypoint.gameObject;
        }

        void CreateBranch()
        {
            GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(WaypointAuthoring));
            waypointObject.transform.SetParent(waypointRoot, false);

            WaypointAuthoring waypoint = waypointObject.GetComponent<WaypointAuthoring>();

            WaypointAuthoring branchedFrom = Selection.activeGameObject.GetComponent<WaypointAuthoring>();
            branchedFrom.branches.Add(waypoint);

            waypoint.branches = new System.Collections.Generic.List<WaypointAuthoring> { branchedFrom };

            waypoint.transform.position = branchedFrom.transform.position;
            waypoint.transform.forward = branchedFrom.transform.forward;

            Selection.activeGameObject = waypoint.gameObject;


        }


        void CreateWaypointBefore()
        {
            GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(WaypointAuthoring));
            waypointObject.transform.SetParent(waypointRoot, false);

            WaypointAuthoring newWaypoint = waypointObject.GetComponent<WaypointAuthoring>();

            WaypointAuthoring selectedWaypoint = Selection.activeGameObject.GetComponent<WaypointAuthoring>();

            waypointObject.transform.position = selectedWaypoint.transform.position;
            waypointObject.transform.forward = selectedWaypoint.transform.forward;

            if (selectedWaypoint.previousWaypoint != null)
            {
                newWaypoint.previousWaypoint = selectedWaypoint.previousWaypoint;
                selectedWaypoint.previousWaypoint.nextWaypoint = newWaypoint;
            }

            newWaypoint.nextWaypoint = selectedWaypoint;

            selectedWaypoint.previousWaypoint = newWaypoint;

            newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());

            Selection.activeGameObject = newWaypoint.gameObject;
        }

        void CreateWaypointAfter()
        {
            GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(WaypointAuthoring));
            waypointObject.transform.SetParent(waypointRoot, false);

            WaypointAuthoring newWaypoint = waypointObject.GetComponent<WaypointAuthoring>();

            WaypointAuthoring selectedWaypoint = Selection.activeGameObject.GetComponent<WaypointAuthoring>();

            waypointObject.transform.position = selectedWaypoint.transform.position;
            waypointObject.transform.forward = selectedWaypoint.transform.forward;

            newWaypoint.previousWaypoint = selectedWaypoint;

            if (selectedWaypoint.nextWaypoint != null)
            {
                newWaypoint.nextWaypoint.previousWaypoint = newWaypoint;
                newWaypoint.nextWaypoint = selectedWaypoint.nextWaypoint;
            }

            selectedWaypoint.nextWaypoint = newWaypoint;

            newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());

            Selection.activeGameObject = newWaypoint.gameObject;
        }

        void RemoveWaypoint()
        {
            WaypointAuthoring selectedWaypoint = Selection.activeGameObject.GetComponent<WaypointAuthoring>();

            if (selectedWaypoint.nextWaypoint != null)
            {
                selectedWaypoint.nextWaypoint.previousWaypoint = selectedWaypoint.previousWaypoint;
            }

            if (selectedWaypoint.previousWaypoint != null)
            {
                selectedWaypoint.previousWaypoint.nextWaypoint = selectedWaypoint.nextWaypoint;
                Selection.activeGameObject = selectedWaypoint.previousWaypoint.gameObject;
            }

            DestroyImmediate(selectedWaypoint.gameObject);
        }
    }
}
