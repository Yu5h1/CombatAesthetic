using UnityEngine;
using static Yu5h1Lib.GameManager.IDispatcher;
using Yu5h1Lib.Game.Character;

public class TriggerCharacterEvent2D : TriggerEvent2D<Controller2D>
{
    public bool HideOnTriggerEnter = true;

    protected override bool OnTriggerEntered2D(Controller2D component)
    {
        if (!base.OnTriggerEntered2D(component))
            return false;
        OnTriggerEnter2DEvent?.Invoke(component);
        if (TryGetComponent(out AudioSource audioSource))
            gameManager.PlayAudio(audioSource);
        if (HideOnTriggerEnter)
            gameObject.SetActive(false);
        return true;
    }
    protected override bool OnTriggerExited2D(Controller2D component)
    {
        if (!base.OnTriggerExited2D(component))
            return false;
        OnTriggerExit2DEvent?.Invoke(component);
        return true;
    }
    public void LoadScene(int index) => SceneController.LoadScene(index);


}
