using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.Game.Character;

[DisallowMultipleComponent]
public class EmojiController : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public SpriteRenderer spriteRenderer => _spriteRenderer;


    //public BaseColliderDetector2D detector;

    /// <summary>
    /// from center
    /// </summary>
    [SerializeField]
    private Vector2 offset = new Vector3(0.5f,0.5f);
 
    Coroutine showEmojiProcCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = new GameObject("emoji").AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 2;
            spriteRenderer.transform.SetParent(transform);
            spriteRenderer.transform.localRotation = Quaternion.identity;
            spriteRenderer.transform.localPosition = offset;
        }
        spriteRenderer.enabled = false;
    }
    public void ShowEmoji(string name,float lifeTime)
    {
        if (ResourcesEx.TryLoad(@$"Texture\{name}", out Sprite sprite))
            this.StartCoroutine(ref showEmojiProcCoroutine, ShowEmojiProc(sprite, lifeTime));
    }
    private IEnumerator ShowEmojiProc(Sprite sprite ,float lifeTime)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.enabled = true;
        yield return new WaitForSeconds(lifeTime);
        spriteRenderer.enabled = false;
    }

    [ContextMenu("Test")]
    private void Test()
    {
        ShowEmoji("exclamation mark", 2);
    }
}
