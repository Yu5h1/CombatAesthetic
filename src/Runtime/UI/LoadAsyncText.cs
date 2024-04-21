
using UnityEngine;

/// <summary>
///  component.text = $"{component.name}{percentage * 100:0.0}%";
/// </summary>
public class LoadAsyncText : LoadAsyncBehaviour<UnityEngine.UI.Text>
{
    public override void OnProcessing(float percentage)
        => component.text = $"{component.name}{percentage * 100:0.0}%";
}
