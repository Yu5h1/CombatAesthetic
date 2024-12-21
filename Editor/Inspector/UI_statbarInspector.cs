using UnityEditor;
using Yu5h1Lib;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(UI_Statbar))]
public class UI_statbarInspector : Editor<UI_Statbar>
{
    AttributeStat testStat;
    private void OnEnable()
    {
        testStat = AttributeStat.Default;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (EditorApplication.isPlaying || targetObject.fills.IsEmpty())
            return;

        if (this.TrySlider("SimulateValue", testStat.normal, 0, 1, out float val))
        {
            testStat.current = val * testStat.max;
            targetObject.Refresh(testStat);
            //force ui image redraw
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
