using System.Collections.Generic;
using QTS.QWorld.Component;
using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.Authoring
{
    public class GridAuthoring : MonoBehaviour
    {
        public Vector2 size = new Vector2(10, 10);
        public float cellSize = 1f;
        public Color gridColor = Color.white;
        public Transform checker;
        public bool showGridGeneration = false;

        private List<GameObject> _cellObjects = new List<GameObject>();

        class Baker : Baker<GridAuthoring>
        {
            public override void Bake(GridAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                AddComponent(entity, new GridComponent()
                {
                    size = authoring.size,
                    cellSize = authoring.cellSize,
                    localTransform = authoring.transform.position
                });
            }
        }

        private void OnDrawGizmos()
        {
            if (showGridGeneration)
                DrawGrid();
        }

        private void DrawGrid()
        {
            Gizmos.color = gridColor;

            Vector3 origin = transform.position;
            int numCellsX = Mathf.FloorToInt(size.x / cellSize);
            int numCellsY = Mathf.FloorToInt(size.y / cellSize);

            for (int x = 0; x <= numCellsX; x++)
            {
                float xPos = x * cellSize;
                Vector3 startPoint = origin + new Vector3(xPos, 0, 0);
                Vector3 endPoint = origin + new Vector3(xPos, 0, numCellsY * cellSize);
                Gizmos.DrawLine(startPoint, endPoint);
            }

            for (int y = 0; y <= numCellsY; y++)
            {
                float yPos = y * cellSize;
                Vector3 startPoint = origin + new Vector3(0, 0, yPos);
                Vector3 endPoint = origin + new Vector3(numCellsX * cellSize, 0, yPos);
                Gizmos.DrawLine(startPoint, endPoint);
            }
        }

        public void GenerateGrid()
        {
            int numCellsX = Mathf.FloorToInt(size.x / cellSize);
            int numCellsY = Mathf.FloorToInt(size.y / cellSize);

            for (int x = 0; x < numCellsX; x++)
            {
                for (int y = 0; y < numCellsY; y++)
                {
                    GameObject cellObject = new GameObject($"Cell_{x}_{y}");
                    cellObject.transform.SetParent(transform);
                    cellObject.transform.localPosition = new Vector3(x * cellSize + cellSize / 2, 0, y * cellSize + cellSize / 2);

                    CellAuthoring cellAuthoring = cellObject.AddComponent<CellAuthoring>();
                    FillCellData(cellAuthoring, x, y);

                    _cellObjects.Add(cellObject);
                }
            }

            // Cập nhật kích thước thực tế của lưới
            size = new Vector2(numCellsX * cellSize, numCellsY * cellSize);
        }

        public Vector2Int GetCellCoordinates(Vector3 position)
        {
            Vector3 localPosition = transform.InverseTransformPoint(position);

            int x = Mathf.FloorToInt(localPosition.x / cellSize);
            int y = Mathf.FloorToInt(localPosition.z / cellSize);

            int numCellsX = Mathf.FloorToInt(size.x / cellSize);
            int numCellsY = Mathf.FloorToInt(size.y / cellSize);

            x = Mathf.Clamp(x, 0, numCellsX - 1);
            y = Mathf.Clamp(y, 0, numCellsY - 1);

            return new Vector2Int(x, y);
        }


        private void FillCellData(CellAuthoring cellAuthoring, int x, int y)
        {
            cellAuthoring.coordinate = new Vector2Int(x, y);
        }
    }
}