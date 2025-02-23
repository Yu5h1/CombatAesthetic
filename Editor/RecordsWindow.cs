using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Yu5h1Lib.Game;
using Yu5h1Lib;
using UnityEngine.SceneManagement;
using DG.DOTweenEditor.UI;
public class RecordsWindow : EditorWindow
{
    private static RecordsWindow m_RecordsWindow;
    private static RecordsWindow window {
        get {
            if (m_RecordsWindow == null) Init();
            return m_RecordsWindow;
        }
    }
	[MenuItem("Tools/Records Window")]
	public static void Init()
	{
        m_RecordsWindow = (RecordsWindow)EditorWindow.GetWindow(typeof(RecordsWindow));
        window.titleContent = new GUIContent("RecordsWindow");
    }
    public static bool expendSaves
    {
        get => EditorPrefs.GetBool($"Expend{nameof(RecordsWindow)}");
        set => EditorPrefs.SetBool($"Expend{nameof(RecordsWindow)}", value);
    }
    public int CurrentSaveSlot
    {
        get => Records.CurrentSaveSlot;
        set => Records.CurrentSaveSlot = value;
    }


    private void OnFocus()
    {
        Records.Load();
    }
    void OnGUI()
	{
        EditorGUI.indentLevel++;
        GUILayout.BeginHorizontal();
        var NewExpendSaves = EditorGUILayout.Foldout(expendSaves, "");

        if (NewExpendSaves != expendSaves)
        {
            expendSaves = NewExpendSaves;
        }
        EditorGUI.BeginDisabledGroup(Records.Saves.Count >= 5);
        if (GUILayout.Button("+", GUILayout.Width(22)))
        {
            Records.Saves.Add(new Record()
            {
                buildIndex = -1,
                position = new Vector3(float.NaN, float.NaN, float.NaN)
            });
            Records.Save();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(Records.Saves.Count == 0);
        if (GUILayout.Button("-", GUILayout.Width(22)))
        {
            Records.Saves.RemoveLast();
            Records.Save();
        }
        EditorGUI.EndDisabledGroup();
        int newSaveIndex = EditorGUILayout.IntField(nameof(CurrentSaveSlot), CurrentSaveSlot);
        if (newSaveIndex != Records.CurrentSaveSlot)
            Records.CurrentSaveSlot = newSaveIndex;
 

        GUILayout.EndHorizontal();

        if (expendSaves)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(3);
            
            var datas = Records.Saves;

            for (int i = 0; i < datas.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 24);
                //var originfieldWidth = EditorGUIUtility.fieldWidth;
                //EditorGUIUtility.fieldWidth = 22;

                //Scene? scene = datas[i].buildIndex < 1 ? null : SceneManager.GetSceneByBuildIndex(datas[i].buildIndex);

                GUILayout.Label($"{i}. {datas[i].name} ", GUILayout.Width(80));
                datas[i].buildIndex = EditorGUILayout.IntField(datas[i].buildIndex, GUILayout.Width(60));

                float originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80;
                //EditorGUIUtility.fieldWidth = originfieldWidth;
                if (datas[i].position.IsNaN())
                    EditorGUILayout.LabelField("NaN");
                else
                    datas[i].position = EditorGUILayout.Vector3Field("Position:", datas[i].position);

                EditorGUIUtility.labelWidth = originalLabelWidth;

                if (GUILayout.Button(datas[i].position.IsNaN() ? "+" : "x"))
                {
                    datas[i].position = datas[i].position.IsNaN() ? Vector3.zero : new Vector3(float.NaN, float.NaN, float.NaN);
                }

                GUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            { 
                Records.Save();
            }
        }
        EditorGUI.indentLevel--;
        GUILayout.Space(3);
    }
    
}
