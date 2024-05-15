using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yu5h1Lib;

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
        var tweener = canvasGroup.DOFade(1, duration);
        tweener.SetUpdate(true);
    }
    public void Engage(bool activate = true)
    {
        if (gameObject.activeSelf != activate)
            gameObject.SetActive(activate);
    }
    public void Dismiss()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
    public void ExitGame() => GameManager.ExitGame();
    public void LoadScene(int index) => GameManager.instance.LoadScene(index);
    public void ReloadCurrentScene() => SceneController.ReloadCurrentScene();
}