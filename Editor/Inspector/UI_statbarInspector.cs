using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(UI_Statbar))]
public class UI_statbarInspector : Editor<UI_Statbar>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (targetObject.fill && this.TrySlider(out float val, targetObject.fill.fillAmount, 0, 1))
        {
            targetObject.fill.fillAmount = val;
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
