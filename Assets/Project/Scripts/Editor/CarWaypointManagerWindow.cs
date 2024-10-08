using QTS.QWorld.Authoring;
using UnityEditor;
using UnityEngine;
namespace QTS.QWorld.Editor
{
    public class CarWaypointManagerWindow : EditorWindow
    {
        private Transform carWaypointRoot;

        [MenuItem("Tools/CarWaypoint Editor")]
        public static void OpenWindow()
        {
            GetWindow<CarWaypointManagerWindow>("CarWaypoint Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("CarWaypoint Manager", EditorStyles.boldLabel);

            carWaypointRoot = EditorGUILayout.ObjectField("CarWaypoint Root", carWaypointRoot, typeof(Transform), true) as Transform;

            if (carWaypointRoot == null)
            {
                EditorGUILayout.HelpBox("Please assign a root transform for the CarWaypoint.", MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("Create CarWaypoint"))
                {
                    CreateWaypoint();
                }

                if (GUILayout.Button("Clear All CarWaypoint"))
                {
                    ClearAllWaypoints();
                }

                if (GUILayout.Button("Show Name"))
                {
                    ShowName();
                }

                if (GUILayout.Button("Hide Name"))
                {
                    HideName();
                }
            }
        }

        void ShowName()
        {
            foreach (Transform child in carWaypointRoot.transform)
            {
                var script = child.GetComponent<CarWaypoint>();
                script.isShowName = true;
            }
        }

        void HideName()
        {
            foreach (Transform child in carWaypointRoot.transform)
            {
                var script = child.GetComponent<CarWaypoint>();
                script.isShowName = false;
            }
        }

        private void CreateWaypoint()
        {
            GameObject waypointObject = new GameObject("CarWaypoint " + carWaypointRoot.transform.childCount);
            waypointObject.transform.SetParent(carWaypointRoot, false);
            waypointObject.AddComponent<CarWaypoint>();

            Selection.activeGameObject = waypointObject;
        }

        private void ClearAllWaypoints()
        {
            while (carWaypointRoot.childCount > 0)
            {
                DestroyImmediate(carWaypointRoot.GetChild(0).gameObject);
            }
        }
    }


}

[CustomEditor(typeof(CarWaypoint))]
public class CarWaypointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CarWaypoint waypoint = (CarWaypoint)target;

        if (GUILayout.Button("Add Next CarWaypoint"))
        {
            GameObject newCarWaypointObj = new GameObject("CarWaypoint " + waypoint.transform.parent.childCount);
            newCarWaypointObj.transform.position = waypoint.transform.position;
            newCarWaypointObj.transform.rotation = waypoint.transform.rotation;

            newCarWaypointObj.transform.SetParent(waypoint.transform.parent, false);

            CarWaypoint newCarWaypoint = newCarWaypointObj.AddComponent<CarWaypoint>();
            newCarWaypoint.isRightSide = waypoint.isRightSide;

            waypoint.branches.Add(new Branch()
            {
                carWaypoint = newCarWaypoint,
            });

            // Select the new waypoint
            Selection.activeGameObject = newCarWaypointObj;

            // Ensure the hierarchy window updates to show the selection
            EditorApplication.DirtyHierarchyWindowSorting();

            // This will make sure the inspector updates for the new selection
            EditorUtility.SetDirty(newCarWaypoint);
        }
    }
}