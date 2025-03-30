using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Agent : MonoBehaviour
{
    UI_Manager manager => UI_Manager.instance;

    public void ChangeMenu(string MenuName, bool close) => manager.ChangeMenu(MenuName, close);
    public void SwitchMenu(string MenuName) => ChangeMenu(MenuName, true);
    public void Popup(string MenuName) => ChangeMenu(MenuName, false);

    public void Prompt(Sprite sprite)
    {
        Popup("Prompt");
        UI_Manager.currentMenu.GetComponent<UI_Prompt>().Prompt(sprite);
    }
}
