using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yu5h1Lib;
using static SceneController;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Menu : UI_Behaviour
{
    public bool activeSelf => gameObject.activeSelf;
    [SerializeField] private Button _Submit;
    public Button Submit => _Submit ?? TryFindButton(nameof(Submit), ref _Submit);
    [SerializeField] private Button _Cancel;
    public Button Cancel => _Cancel ?? TryFindButton(nameof(Cancel), ref _Cancel);
    [SerializeField] private CanvasGroup _CanvasGroup;
    public CanvasGroup canvasGroup => _CanvasGroup;

    private void Reset()
    {
        TryFindButton(nameof(Submit), ref _Submit);
        TryFindButton(nameof(Cancel), ref _Cancel);
        TryGetComponent(out _CanvasGroup);
    }
    private void Start()
    {
        TryGetComponent(out _CanvasGroup);
    }
    public Button TryFindButton(string name, ref Button button)
    {
        if (!rectTransform.TryGetComponentInChildren(name, out button))
            Debug.LogWarning($"{name} name:{name} Botton could not be found !");
        return button;
    }
    public void FadeIn(float duration)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, duration);
    }
    public void Engage(bool activate = true)
    {
        if (gameObject.activeSelf != activate)
            gameObject.SetActive(activate);
        if (Submit)
        {
            Submit.onClick.RemoveAllListeners();
            ((Text)Submit.targetGraphic).text = IsStartScene ? "Start" : "Restart";
            Submit.onClick.AddListener(OnSubmitClick);
        }
        if (Cancel)
        {
            Cancel.onClick.RemoveAllListeners();
            ((Text)Cancel.targetGraphic).text = IsStartScene ? "Exit" : "Main Menu";
            Cancel.onClick.AddListener(OnCancelClick);
        }
    }
    public void Dismiss()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        if (Submit)
            Submit.onClick.RemoveAllListeners();
        if (Cancel)
            Cancel.onClick.RemoveAllListeners();
    }
    private static void OnSubmitClick()
    {
        if (IsStartScene)
            LoadScene(1);
        else
            ReloadCurrentScene();
    }
    private static void OnCancelClick()
    {
        if (IsStartScene)
            GameManager.ExitGame();
        else
            GameManager.instance.LoadScene(0);
    }
}