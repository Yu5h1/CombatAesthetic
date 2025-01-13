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

        // ����I����m
        Vector3[] positions = new Vector3[positionCount];
        lineRenderer.GetPositions(positions);

        // ���I�B�k�u�M�T���ί��޺c��
        Vector3[] vertices = new Vector3[positionCount * 2];
        Vector3[] normals = new Vector3[positionCount * 2]; // �k�u
        int[] triangles = new int[(positionCount - 1) * 6];

        float width = lineRenderer.startWidth; // ���]�e�׬O�T�w��

        for (int i = 0; i < positionCount; i++)
        {
            // �p��C�ӳ��I�����k�k�u
            Vector3 dir = Vector3.zero;
            if (i > 0) dir += (positions[i] - positions[i - 1]).normalized;
            if (i < positionCount - 1) dir += (positions[i + 1] - positions[i]).normalized;

            // ������u�q����V�]����/�k���^
            dir = Vector3.Cross(dir.normalized, Vector3.forward).normalized * (width / 2);

            // �p�⥪���M�k�����I����m
            vertices[i * 2] = positions[i] - dir; // �������I
            vertices[i * 2 + 1] = positions[i] + dir; // �k�����I

            // �p��������I���k�u��V
            normals[i * 2] = -dir.normalized; // �������I�k�u
            normals[i * 2 + 1] = dir.normalized; // �k�����I�k�u
        }

        // �إߤT���ί���
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

        // �]�w Mesh ���
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals; // �]�w�k�u
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
    //    // ����I����m
    //    Vector3[] positions = new Vector3[positionCount];
    //    lineRenderer.GetPositions(positions);

    //    // ���I�M�T���ί��޺c��
    //    Vector3[] vertices = new Vector3[positionCount * 2];
    //    int[] triangles = new int[(positionCount - 1) * 6];

    //    float width = lineRenderer.startWidth; // ���]�e�׬O�T�w��

    //    for (int i = 0; i < positionCount; i++)
    //    {
    //        Vector3 dir = Vector3.zero;
    //        if (i > 0) dir += (positions[i] - positions[i - 1]).normalized;
    //        if (i < positionCount - 1) dir += (positions[i + 1] - positions[i]).normalized;
    //        dir = Vector3.Cross(dir.normalized, Vector3.forward) * (width / 2); // ���] Z �b�����V

    //        vertices[i * 2] = positions[i] - dir; // �������I
    //        vertices[i * 2 + 1] = positions[i] + dir; // �k�����I
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

    //    // �]�w Mesh ���
    //    mesh.Clear();
    //    mesh.vertices = vertices;
    //    mesh.triangles = triangles;
    //    return mesh;
    //}
}
