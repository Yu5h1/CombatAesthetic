using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(PoolElementHandler))]
public class PoolElementEditor : Editor<PoolElementHandler>
{
    public override void OnInspectorGUI()
    {
        this.Iterate(OnProperty, BeginDrawProperty);

    }
    private void BeginDrawProperty()
    {
        
    }
    public void OnProperty(SerializedProperty property)
    {
        if (property.name == "element")
        {
            Component source = null;
            if (targetObject.element)
                PoolManager.element_source_Maps.TryGetValue(targetObject.element, out source);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Source", source, typeof(Component), true);
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.PropertyField(property);
    }
}