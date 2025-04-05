using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

[RequireComponent(typeof(SpriteMask))]
public class SpriteMaskAdapter : BaseMonoBehaviour
{
    [SerializeField, ReadOnly]
    private SpriteMask _spriteMask;
    public SpriteMask spriteMask => _spriteMask;

    [SerializeField]
    private SpriteRenderer _targetSpriteRenderer;
    public SpriteRenderer targetSpriteRenderer => _targetSpriteRenderer;

    private void Reset()
    {
        Init();
    }
    protected override void OnInitializing()
    {
        this.GetComponent(ref _spriteMask);
    }
    [ContextMenu(nameof(UpdateMaskSize))]
    public void UpdateMaskSize()
    {
        if (spriteMask == null || targetSpriteRenderer == null || targetSpriteRenderer.sprite == null || spriteMask.sprite == null)
            return;

        Vector2 targetSize = targetSpriteRenderer.sprite.bounds.size;
        Vector2 maskSize = spriteMask.sprite.bounds.size;
        Vector3 scaleFactor = new Vector3(targetSize.x / maskSize.x, targetSize.y / maskSize.y, 1);
        transform.localScale = scaleFactor;
    }

}
