using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yu5h1Lib;
using Yu5h1Lib.UI;

public class UI_Prompt : UIControl
{
    [SerializeField]
    private Image _image;
    [SerializeField]
    private TextAdapter _textAdapter;

    protected override void OnInitializing()
    {
        base.OnInitializing();
        this.GetComponent(ref _image);
        this.GetComponent(ref _textAdapter);
    }

    public void Prompt(Sprite sprite)
    {
        _image.sprite = sprite;
        _image.gameObject.SetActive(true);
    }
}
