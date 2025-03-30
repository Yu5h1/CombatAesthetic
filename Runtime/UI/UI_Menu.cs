using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yu5h1Lib;
using Yu5h1Lib.UI;
using Yu5h1Lib.Game;
using UnityEngine.Events;

public class UI_Menu : UIControl
{
    public static UI_Manager manager => UI_Manager.instance;

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

    public UnityEvent shown;

    private void OnEnable()
    {

    }
    public void Engage()
    {
        UI_Manager.Engage(this);
        shown?.Invoke();
    }
    public void Dismiss(bool force = false)
    {
        if (DisallowDismiss && !force)
            return;
        UI_Manager.Dismiss(this);
    }

    public void ChangeMenu(UI_Menu next, bool dismiss) => UI_Manager.ChangeMenu(this, next, dismiss);
    public void SwitchMenu(UI_Menu next) => ChangeMenu(next, true);
    public void Popup(UI_Menu next) => ChangeMenu(next, false);

    public void ChangeMenu(string MenuName, bool close) => manager.ChangeMenu(MenuName, close);
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
    public void ReloadCurrentScene() 
        => SceneController.ReloadCurrentScene();

    public void ActiveIfHasAnyRecords(Selectable control) => UI_Manager.ActiveIfHasAnyRecords(control);
    public void SetInteractableIfHasAnyRecords(Selectable control) => UI_Manager.SetInteractableIfHasAnyRecords(control);

    public void SetGamePause(bool pause) => GameManager.IsGamePaused = pause;

    public void ToggleActive(GameObject obj) => obj.SetActive(!obj.activeSelf);

    public void MoveToLast() => UI_Manager.MoveToLast(transform);

    public void ActivateCurrntSaveSelection(LayoutGroup group)
    {
        var selectables = group.GetComponentsInChildren<Selectable>();
        if ("Items are empty.".printWarningIf(selectables.IsEmpty()))
            return;
        if ($"{Records.CurrentSaveSlot} is out of range[{selectables.Length}].".printWarningIf(!selectables.IsValid(Records.CurrentSaveSlot)))
            return;
        selectables[Records.CurrentSaveSlot].Select();
    }
    public void SetSaveSelection(Selectable selectable)
    {
        Records.CurrentSaveSlot = selectable.transform.GetSiblingIndex();
    }
    public void LoadRecords() => Records.Load();

    public void LoadSlotRecord(Selectable selectable)
    {
        var record = Records.Saves[selectable.transform.GetSiblingIndex()];
        record.Load();
    }
    public void ReadSlotRecord(Selectable selectable)
    {
        var textAdapter = selectable.GetComponent<TextAdapter>();
        if ($"{selectable}'s TextAdapter does not exist.".printWarningIf(!textAdapter))
            return;
        var index = selectable.transform.GetSiblingIndex();
        if (!Records.Saves.IsValid(index))
            Records.Prepare(index);

        var record = Records.Saves[index];
        textAdapter.text = $"{record}";     
    }
    public void ClearSlotRecord(GroupHandler group)
    {
        var selectable = group.GetComponentsInChildren<Selectable>()[Records.CurrentSaveSlot];
        var textAdapter = selectable.GetComponent<TextAdapter>();
        if ($"{selectable}'s TextAdapter does not exist.".printWarningIf(!textAdapter))
            return;
        var record = Records.Saves[selectable.transform.GetSiblingIndex()];
        record.Clear();
        textAdapter.text = $"{record}";
        Records.Save();
    }
}