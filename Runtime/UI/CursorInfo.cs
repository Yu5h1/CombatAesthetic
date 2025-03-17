using UnityEngine;
using Yu5h1Lib.Runtime;

[System.Serializable]
public class CursorInfo
{
    public Texture2D texture;
    
    public Vector2 offset = new Vector2(-1,-1);
    public bool useOffset
    {
        get => !(offset.x < 0 || offset.y < 0);
        set 
        {
            if (useOffset == value)
                return;
            offset = value ? Vector2.zero  : - Vector2.one;
        }
    }
    public Alignment alignment;
    public CursorMode mode = CursorMode.Auto;

    public void Use()
    {
        if ("Cursor was undefined !".printWarningIf(!texture))
            return;
        Cursor.SetCursor(texture, useOffset ? offset : GetHotspot(texture,alignment), mode);
    }

    private static Vector2 GetHotspot(Texture2D cursorTexture, Alignment alignment)
    {
        if (cursorTexture == null) return Vector2.zero;
        switch (alignment)
        {
            case Alignment.Top: return new Vector2(cursorTexture.width / 2, 0);
            case Alignment.Bottom: return new Vector2(cursorTexture.width / 2, cursorTexture.height);
            case Alignment.Left: return new Vector2(0, cursorTexture.height/2);
            case Alignment.Right: return new Vector2(cursorTexture.width, cursorTexture.height / 2);
            case Alignment.Top_Left: return new Vector2(0, 0);
            case Alignment.Top_Right: return new Vector2(cursorTexture.width, 0);
            case Alignment.Bottom_Left: return new Vector2(0, cursorTexture.height);
            case Alignment.Bottom_Right: return new Vector2(cursorTexture.width, cursorTexture.height);
            case Alignment.Center: return new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
            default: return new Vector2(0, 0);
        }
    }
}
