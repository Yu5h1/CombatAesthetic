using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class FadeSpriteRenderer : BaseMonoBehaviour
{
    private SpriteRenderer[] spriteRenderers;
    private Timer timer;

    Coroutine coroutine;
    protected override void OnInitializing()
    {
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        timer = new Timer();
    }

    public void SetSpriteRenderersColor(Color color)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].color = color;
    }
    public void SetSpriteRenderersAlpha(float alpha)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
            spriteRenderers[i].color = spriteRenderers[i].color.ChangeAlpha(alpha);
    }

    public void FadeIn()
    {
        //this.StartCoroutine(ref coroutine, Fade(Color.white,Color))
    }
    public void FadeOut()
    {

    }
    //private IEnumerator Fade(bool InOut, float duration, System.Action completed)
    //{
    //    SetSpriteRenderersColor(start);
    //    timer.duration = duration;
    //    timer.Start();
    //    while (!timer.IsCompleted) 
    //    {
    //        SetSpriteRenderersColor(Color.Lerp(start,end,timer.normalized));
    //        yield return null;
    //    }
    //    completed?.Invoke();
    //}


}
