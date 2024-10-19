using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Text;
using System.Linq;
using TMPro;

[CustomEditor(typeof(UI_Dialog_TMP))]
public class UI_DialogInspector : Editor<UI_Dialog_TMP>
{
    //static char[] InvalidChars = new char[] { '\n', '\r', '\t', '\b', '\f', '\\', '\'', '\"', '\0', '\v' };
    //SerializedObject serializedTarget;
    void OnEnable()
    {
        //serializedTarget = new SerializedObject(target);
    }
    public override void OnInspectorGUI()
    {
        //serializedTarget.Update();
        base.OnInspectorGUI();
        if (GUILayout.Button("Add element from Content"))
        {
            targetObject.AddElementFromContent();
            EditorUtility.SetDirty(target);
        }
        if (GUILayout.Button("Check & Add Font Text"))
        {
            var charactersFilePath = Path.Combine(Application.dataPath, "Resources", "Font", "Characters.txt");
            if (File.Exists(charactersFilePath)) {
                var content = File.ReadAllText(charactersFilePath);
                var stringBuilder = new StringBuilder(content);
                
                foreach (var c in targetObject.Content)
                {
                    //if (InvalidChars.Contains(c))
                    //    continue;
                    if (!content.Contains(c)) {
                        var index = (int)c;
                        if (index > content.Length)
                            index = content.Length;
                        stringBuilder.Insert(index, c);
                        content = stringBuilder.ToString();
                    }
                }

                var sortedBuilder = new StringBuilder();
                foreach (var c in content.OrderBy(c => (int)c).Reverse())
                    sortedBuilder.Append(c);

                File.WriteAllText(charactersFilePath, sortedBuilder.ToString());


                //TMPro_FontAssetCreatorWindow

            }
        }
    }
}


//[ContextMenu("Check&Add Font Text")]
//public void CheckFontText()
//{
//    var so = new UnityEditor.SerializedObject(this);

//    var persistentCalls = so.FindProperty("OnTriggerEnter2DEvent.m_PersistentCalls.m_Calls");
//    for (int i = 0; i < persistentCalls.arraySize; ++i)
//    {
//        var prop = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_StringArgument");
//        if (prop != null)
//            Debug.Log(prop.stringValue);
//    }

//}
