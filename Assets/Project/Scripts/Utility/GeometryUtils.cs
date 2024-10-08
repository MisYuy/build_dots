using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

namespace QTS.QWorld.Utility
{
    [BurstCompile]
    public static class GeometryUtils
    {
        [BurstCompile]
        public static bool TestPlanesAABB(in float4x4 viewProjectionMatrix, in float3 center, in float3 extents)
        {
            var planes = new NativeArray<float4>(6, Allocator.Temp);
            CalculateFrustumPlanes(in viewProjectionMatrix, ref planes);

            for (int i = 0; i < 6; i++)
            {
                float3 normal = planes[i].xyz;
                float distance = planes[i].w;

                float distanceToCenter = math.dot(normal, center) + distance;
                float radius = math.dot(math.abs(normal), extents);

                if (distanceToCenter + radius < 0)
                {
                    planes.Dispose();
                    return false;
                }
            }

            planes.Dispose();
            return true;
        }

        [BurstCompile]
        private static void CalculateFrustumPlanes(in float4x4 mat, ref NativeArray<float4> planes)
        {
            // Left
            planes[0] = new float4(
                mat.c0.w + mat.c0.x,
                mat.c1.w + mat.c1.x,
                mat.c2.w + mat.c2.x,
                mat.c3.w + mat.c3.x);

            // Right
            planes[1] = new float4(
                mat.c0.w - mat.c0.x,
                mat.c1.w - mat.c1.x,
                mat.c2.w - mat.c2.x,
                mat.c3.w - mat.c3.x);

            // Bottom
            planes[2] = new float4(
                mat.c0.w + mat.c0.y,
                mat.c1.w + mat.c1.y,
                mat.c2.w + mat.c2.y,
                mat.c3.w + mat.c3.y);

            // Top
            planes[3] = new float4(
                mat.c0.w - mat.c0.y,
                mat.c1.w - mat.c1.y,
                mat.c2.w - mat.c2.y,
                mat.c3.w - mat.c3.y);

            // Near
            planes[4] = new float4(
                mat.c0.w + mat.c0.z,
                mat.c1.w + mat.c1.z,
                mat.c2.w + mat.c2.z,
                mat.c3.w + mat.c3.z);

            // Far
            planes[5] = new float4(
                mat.c0.w - mat.c0.z,
                mat.c1.w - mat.c1.z,
                mat.c2.w - mat.c2.z,
                mat.c3.w - mat.c3.z);

            for (int i = 0; i < 6; i++)
            {
                float length = math.length(planes[i].xyz);
                planes[i] /= length;
            }
        }
    }
}
