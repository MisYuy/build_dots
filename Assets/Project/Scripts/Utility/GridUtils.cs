using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace QTS.QWorld.Utility
{
    [BurstCompile]
    public static class GridUtils
    {
        [BurstCompile]
        public static void GetCellCoordinates(in float3 worldPos, in float3 gridOrigin, in float2 gridSize, float cellSize, out int2 result)
        {
            // Chuyển đổi từ _world space sang local space
            float3 localPos = worldPos - gridOrigin;

            // Tính toán tọa độ ô, xử lý cả trường hợp âm và dương
            int x = (int)math.floor(localPos.x / cellSize);
            int y = (int)math.floor(localPos.z / cellSize);

            // Tính số lượng ô trong grid
            int numCellsX = (int)math.floor(gridSize.x / cellSize);
            int numCellsY = (int)math.floor(gridSize.y / cellSize);

            // Đảm bảo tọa độ nằm trong phạm vi của grid
            x = math.clamp(x, 0, numCellsX - 1);
            y = math.clamp(y, 0, numCellsY - 1);

            result = new int2(x, y);
        }

        [BurstCompile]
        public static void GetCellEntityByCoordinate(in int2 coor, in NativeKeyValueArrays<int2, Entity> cellEntities, out Entity result)
        {
            result = Entity.Null;
            for (int i = 0; i < cellEntities.Length; i++)
            {
                if (cellEntities.Keys[i].Equals(coor))
                {
                    result = cellEntities.Values[i];
                    break;
                }
            }
        }
    }
}