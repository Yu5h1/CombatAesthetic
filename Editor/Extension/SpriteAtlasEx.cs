using System.IO;
using UnityEditor;
using UnityEngine;
//using UnityEditor.U2D;
//using UnityEngine.U2D;
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
public static class SpriteAtlasEx
{
    [MenuItem("CONTEXT/SpriteAtlasImporter/ExportAtlasAsPNG")]
    public static void ExportAtlasAsPNG(MenuCommand command)
    {
        //if (!(command.context is SpriteAtlas spriteAtlas))
        //    return;
        //var atlasTexture = spriteAtlas.GetSprite("YourSpriteName").texture;

        //Texture2D texture = new Texture2D(atlasTexture.width, atlasTexture.height, atlasTexture.format, false);
        //RenderTexture rt = RenderTexture.GetTemporary(atlasTexture.width, atlasTexture.height);

        //Graphics.Blit(atlasTexture, rt);
        //RenderTexture.active = rt;

        //texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        //texture.Apply();

        //byte[] bytes = texture.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/SpriteAtlasExport.png", bytes);

        //RenderTexture.ReleaseTemporary(rt);
        //Debug.Log("Atlas exported as PNG to: " + Application.dataPath + "/SpriteAtlasExport.png");
    }
}