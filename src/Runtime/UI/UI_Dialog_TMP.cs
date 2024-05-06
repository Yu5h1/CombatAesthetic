using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Dialog_TMP : UI_DialogBase
{
    public TextMeshProUGUI textMeshProUGUI;
    public override string Content { get => textMeshProUGUI.text; set => textMeshProUGUI.text = value; }
    
    private void Reset()
    {
        if (!TryGetComponent(out textMeshProUGUI))
            textMeshProUGUI = GetComponentInChildren<TextMeshProUGUI>();
    }
}
