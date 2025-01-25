using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib;

public static class LineRendererEx
{
    public static float GetLengthNormalized(this LineRenderer lineRenderer,int index)
    {
        var totalLength = lineRenderer.GetLength();
        var pointLength = lineRenderer.GetLength(endIndex:index);

        if (pointLength <= 0f)
            throw new InvalidOperationException("Point length is zero or negative.");

        return totalLength / pointLength;
    }
    public static float GetLength(this LineRenderer lineRenderer , int startIndex = 0,int endIndex = -1)
    {
        
        if (lineRenderer == null)
            throw new ArgumentNullException(nameof(lineRenderer));
        var lastIndex = lineRenderer.positionCount - 1;
        endIndex = (endIndex < startIndex || endIndex > lastIndex) ? lastIndex : endIndex;
        if (endIndex < startIndex)
            return 0;

        var length = 0f;
        for (int i = startIndex; i < endIndex; i++)
            length += Vector3.Distance(lineRenderer.GetPosition(i) , lineRenderer.GetPosition(i + 1));
        return length;
    }

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
    public static void SetGradient(this LineRenderer lineRenderer, Color start ,Color end )
    {
        
        var g = lineRenderer.colorGradient;

        //var firstColor = lineRenderer.colorGradient.colorKeys.IsEmpty() ?
        //                 defaultColor : lineRenderer.colorGradient.colorKeys.First().color;
        //var lastColor = lineRenderer.colorGradient.colorKeys.IsEmpty() ?
        //                defaultColor : lineRenderer.colorGradient.colorKeys.Last().color;
        //var start = startColorParam ?? firstColor;
        //var end = endColorParam ?? firstColor;

        var alphakeys = lineRenderer.colorGradient.alphaKeys;
        var colorkeys = lineRenderer.colorGradient.colorKeys;

        if (alphakeys.Length != 8)
            Array.Resize(ref alphakeys, 8); 
        if (colorkeys.Length != 8)
            Array.Resize(ref colorkeys, 8);

        var interval = 1.0f / 7.0f;
        var t = 0f;
        for (int i = 0; i < 8; i++)
        {
            alphakeys[i] = g.alphaKeys[0];
            colorkeys[i].time = alphakeys[i].time = t;
            alphakeys[i].alpha = Mathf.Lerp(start.a, end.a, t);
            colorkeys[i].color = Color.Lerp(start,end,t);
            t += interval;
        }
        g.SetKeys(colorkeys, alphakeys);
        lineRenderer.colorGradient = g;
    }
    /// <summary>
    /// set 8 keys for Gradient
    /// </summary>
    public static void PrepareGradient(this LineRenderer lineRenderer, Color? defaultColor = null)
    {
        var g = lineRenderer.colorGradient;

        var colorKeys = g.colorKeys;
        var alphaKeys = g.alphaKeys;

        if (alphaKeys.Length == 8 && colorKeys.Length == 8)
            return;

        var start = defaultColor ?? (colorKeys.IsEmpty() ? Color.white : colorKeys.First().color);
        var end = defaultColor ?? (colorKeys.IsEmpty() ? Color.white : colorKeys.Last().color);

        lineRenderer.SetGradient(start,end);
    }



}
