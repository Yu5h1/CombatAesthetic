using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Yu5h1Lib;
using Yu5h1Lib.UI;

public class UI_Prompt : UIControl
{
    [SerializeField]
    private Image _image;
    [SerializeField,FormerlySerializedAs("_textAdapter")]
    private TextAddon _textComponent;

    protected override void OnInitializing()
    {
        base.OnInitializing();
        this.GetComponent(ref _image);
        this.GetComponent(ref _textComponent);
    }

    public void Prompt(Sprite sprite)
    {
        _image.sprite = sprite;
        _image.gameObject.SetActive(true);
    }
}
