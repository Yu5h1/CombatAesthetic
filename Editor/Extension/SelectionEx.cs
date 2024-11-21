using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using Yu5h1Lib.EditorExtension;

namespace Yu5h1Lib.EditorExtension
{
    public static class SelectionEx
    {
        public static string SelectedAssetPath
        { get { if (Selection.activeObject != null) {return AssetDatabase.GetAssetPath(Selection.activeObject);} return ""; } }

        public static bool IsMonoBehaviourSelected { get { return IsMonoScriptSubclassOf(typeof(MonoBehaviour)); } }
        public static bool IsMonoScriptSelected
        {
            get
            {
                return Selection.activeObject != null && 
                        Selection.activeObject.GetType() == typeof(MonoScript);
            }
        }

        public static bool IsScriptableObjectScriptSelected { get { return IsMonoScriptSubclassOf(typeof(ScriptableObject)); } }

        public static System.Type GetSelectedScriptClass
        {
            get
            {
                if (IsMonoScriptSelected) return ((MonoScript)Selection.activeObject).GetClass();
                return null;
            }
        }
        public static bool IsAnyObjectSelected() {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("Select an object", "No object selected", "OK");
                return false;
            }
            return true;
        }
        public static bool IsMonoScriptSubclassOf(System.Type type)
        {
            if (IsMonoScriptSelected)
            {
                var classtype = ((MonoScript)Selection.activeObject).GetClass();
                if (classtype != null) {
                    return ((MonoScript)Selection.activeObject).GetClass().IsSubclassOf(type);
                }
            }
            return false;
        }
    }

} 