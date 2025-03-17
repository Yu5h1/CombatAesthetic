using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;


namespace Yu5h1Lib.EditorExtension
{
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
    public static class MeshFilterEx
    {
        public const string LabelBase = "CONTEXT/MeshFilter/";
        public const string LabelSaveMesh = LabelBase + "Save Mesh as asset";

        [MenuItem(LabelSaveMesh)]
        public static void SaveMeshAsAsset(this MenuCommand command)
        {
            var filter = (MeshFilter)command.context;

            AssetDatabase.CreateAsset(filter.sharedMesh, $"Assets/{filter.transform.name}.asset");
            AssetDatabase.SaveAssets();
        }
    }
}

