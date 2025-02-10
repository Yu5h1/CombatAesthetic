using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    public Image AvatarLeft;
    public Image AvatarRight;

    public UI_DialogBase dialog_UI;

    public void Prompt(string[] lines)
    {
        dialog_UI.lines = lines;
        dialog_UI.transform.SetAsLastSibling();
        dialog_UI.gameObject.SetActive(true);
    }
    public void Prompt(string content) => Prompt(content.Split('\n', '\r'));
}
