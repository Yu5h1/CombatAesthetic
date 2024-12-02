using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yu5h1Lib;

public class UI_Menu : UI_Behaviour
{
    public bool DisallowPreviouse;
    public bool DisallowDismiss;
    [ReadOnly,SerializeField]
    private UI_Menu _previous;
    public UI_Menu previous { get => _previous; 
        set {
            if (_previous == this || _previous == value)
                return;
            _previous = value;
        } 
    }

    public bool activeSelf => gameObject.activeSelf;

    private void OnEnable()
    {

    }
    public void Engage()
    {
        UI_Manager.Engage(this);
    }
    public void Dismiss(bool force = false)
    {
        if (DisallowDismiss && !force)
            return;
        UI_Manager.Dismiss(this);

    }
    /// <summary>
    /// close self after moving to next menu
    /// </summary>
    public void ChangeMenu(UI_Menu next,bool dismiss)
    {
        if (!next)
            return;
        if (!next.DisallowPreviouse && !next.previous)
            next.previous = this;
        if (dismiss && !next.transform.IsChildOf(transform))
            Dismiss();
        next.Engage();        
    }
    public void SwitchMenu(UI_Menu menu) => ChangeMenu(menu, true);
    public void Popup(UI_Menu popupmenu) => ChangeMenu(popupmenu,false);

    public void ChangeMenu(string MenuName, bool close)
    {
        if (!manager.TryGetComponentInChildren(MenuName, out UI_Menu menu))
            return;
        ChangeMenu(menu, close);
    }
    public void SwitchMenu(string MenuName) => ChangeMenu(MenuName, true);
    public void Popup(string MenuName) => ChangeMenu(MenuName, false);


    public void ReturnToPrevious()
        => ChangeMenu(previous,true);
    public Button TryFindButton(string name, ref Button button)
    {
        if (!rectTransform.TryGetComponentInChildren(name, out button))
            Debug.LogWarning($"({name}) Botton could not be found !");
        return button;
    }
    public void ExitGame() => GameManager.ExitGame();
    public void StartNewGame() => GameManager.instance.StartNewGame();
    public void LoadScene(int index) => GameManager.instance.LoadScene(index);
    public void ReloadCurrentScene() => SceneController.ReloadCurrentScene();
    public void StartFromCheckPoint() {
        if (!CheckPoint.Load())
            LoadScene(1);
    }
    public void ActiveIfHasAnyRecords(Selectable control)
    {
        if (!control)
            return;
        control.gameObject.SetActive(CheckPoint.Exists);
    }
    public void SetInteractableIfHasAnyRecords(Selectable control)
    {
        if (!control)
            return;
        control.interactable = CheckPoint.Exists || TeleportGate2D.GateStates.Any();
    }
    public void SetGamePause(bool pause) => GameManager.IsGamePause = pause;

}