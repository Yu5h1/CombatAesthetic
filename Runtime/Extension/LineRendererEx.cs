using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LineRendererEx
{
    // Start is called before the first frame upda

    public static Mesh BuildMesh(this LineRenderer lineRenderer, ref Mesh mesh)
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

        // 頂點、法線和三角形索引構建
        Vector3[] vertices = new Vector3[positionCount * 2];
        Vector3[] normals = new Vector3[positionCount * 2]; // 法線
        int[] triangles = new int[(positionCount - 1) * 6];

        float width = lineRenderer.startWidth; // 假設寬度是固定的

        for (int i = 0; i < positionCount; i++)
        {
            // 計算每個頂點的左右法線
            Vector3 dir = Vector3.zero;
            if (i > 0) dir += (positions[i] - positions[i - 1]).normalized;
            if (i < positionCount - 1) dir += (positions[i + 1] - positions[i]).normalized;

            // 垂直於線段的方向（左側/右側）
            dir = Vector3.Cross(dir.normalized, Vector3.forward).normalized * (width / 2);

            // 計算左側和右側頂點的位置
            vertices[i * 2] = positions[i] - dir; // 左側頂點
            vertices[i * 2 + 1] = positions[i] + dir; // 右側頂點

            // 計算對應頂點的法線方向
            normals[i * 2] = -dir.normalized; // 左側頂點法線
            normals[i * 2 + 1] = dir.normalized; // 右側頂點法線
        }

        // 建立三角形索引
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
        mesh.normals = normals; // 設定法線
        mesh.triangles = triangles;

        return mesh;
    }


    //public static Mesh BuildMesh(this LineRenderer lineRenderer,ref Mesh mesh)
    //{
    //    int positionCount = lineRenderer.positionCount;
    //    if (positionCount < 2)
    //    {
    //        Debug.LogWarning("LineRenderer has too few points to bake a mesh.");
    //        return null;
    //    }
    //    // 獲取點的位置
    //    Vector3[] positions = new Vector3[positionCount];
    //    lineRenderer.GetPositions(positions);

    //    // 頂點和三角形索引構建
    //    Vector3[] vertices = new Vector3[positionCount * 2];
    //    int[] triangles = new int[(positionCount - 1) * 6];

    //    float width = lineRenderer.startWidth; // 假設寬度是固定的

    //    for (int i = 0; i < positionCount; i++)
    //    {
    //        Vector3 dir = Vector3.zero;
    //        if (i > 0) dir += (positions[i] - positions[i - 1]).normalized;
    //        if (i < positionCount - 1) dir += (positions[i + 1] - positions[i]).normalized;
    //        dir = Vector3.Cross(dir.normalized, Vector3.forward) * (width / 2); // 假設 Z 軸為正向

    //        vertices[i * 2] = positions[i] - dir; // 左側頂點
    //        vertices[i * 2 + 1] = positions[i] + dir; // 右側頂點
    //    }

    //    for (int i = 0; i < positionCount - 1; i++)
    //    {
    //        int baseIndex = i * 2;
    //        int triIndex = i * 6;

    //        triangles[triIndex] = baseIndex;
    //        triangles[triIndex + 1] = baseIndex + 2;
    //        triangles[triIndex + 2] = baseIndex + 1;

    //        triangles[triIndex + 3] = baseIndex + 1;
    //        triangles[triIndex + 4] = baseIndex + 2;
    //        triangles[triIndex + 5] = baseIndex + 3;
    //    }

    //    // 設定 Mesh 資料
    //    mesh.Clear();
    //    mesh.vertices = vertices;
    //    mesh.triangles = triangles;
    //    return mesh;
    //}
}
