using UnityEngine;
using UnityEditor;
using QTS.QWorld.Authoring;

namespace QTS.QWorld.Editor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(GridAuthoring))]
    public class GridEditorInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GridAuthoring gridEditor = (GridAuthoring)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Grid"))
            {
                gridEditor.GenerateGrid();
            }

            // Ensure the hierarchy window updates to show the selection
            EditorApplication.DirtyHierarchyWindowSorting();

            // This will make sure the inspector updates for the new selection
            EditorUtility.SetDirty(gridEditor);
        }
    }
#endif
}