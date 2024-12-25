using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LineRendererEx
{
    // Start is called before the first frame upda

    public static Mesh BuildMesh(this LineRenderer lineRenderer,ref Mesh mesh)
    {
        int positionCount = lineRenderer.positionCount;
        if (positionCount < 2)
        {
            Debug.LogWarning("LineRenderer has too few points to bake a mesh.");
            return null;
        }
        // 獲取點的位置
        Vector3[] positions = new Vector3[positionCount];
        lineRenderer.GetPositions(positions);

        // 頂點和三角形索引構建
        Vector3[] vertices = new Vector3[positionCount * 2];
        int[] triangles = new int[(positionCount - 1) * 6];

        float width = lineRenderer.startWidth; // 假設寬度是固定的

        for (int i = 0; i < positionCount; i++)
        {
            Vector3 dir = Vector3.zero;
            if (i > 0) dir += (positions[i] - positions[i - 1]).normalized;
            if (i < positionCount - 1) dir += (positions[i + 1] - positions[i]).normalized;
            dir = Vector3.Cross(dir.normalized, Vector3.forward) * (width / 2); // 假設 Z 軸為正向

            vertices[i * 2] = positions[i] - dir; // 左側頂點
            vertices[i * 2 + 1] = positions[i] + dir; // 右側頂點
        }

        for (int i = 0; i < positionCount - 1; i++)
        {
            int baseIndex = i * 2;
            int triIndex = i * 6;

            triangles[triIndex] = baseIndex;
            triangles[triIndex + 1] = baseIndex + 2;
            triangles[triIndex + 2] = baseIndex + 1;

            triangles[triIndex + 3] = baseIndex + 1;
            triangles[triIndex + 4] = baseIndex + 2;
            triangles[triIndex + 5] = baseIndex + 3;
        }

        // 設定 Mesh 資料
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }
}
