using System;
using dImage = System.Drawing.Image;
using Bitmap = System.Drawing.Bitmap;
using FrameDimension = System.Drawing.Imaging.FrameDimension;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public static class TextureImporterEx 
{

    [MenuItem("CONTEXT/TextureImporter/Gif2AnimClip")]
    public static void Gif2AnimClip(MenuCommand command)
    {
        var importer = (TextureImporter)command.context;
        if (!importer.assetPath.EndsWith("gif", StringComparison.OrdinalIgnoreCase))
            return;
        var folder = Path.GetDirectoryName(importer.assetPath);
        var name = Path.GetFileNameWithoutExtension(importer.assetPath);
        if (!TryParseGif(dImage.FromFile(importer.assetPath), out Bitmap[] maps, out float[] times))
            return;

        //Debug.Log(string.Join(',', times));
        //return;
        var spritesPath = maps.Select((map,i) => Path.Combine(folder, $"{name}{i:00}.png")).ToArray();
        
        for (int i = 0; i < maps.Length; i++)
            maps[i].Save(spritesPath[i]);
        AssetDatabase.Refresh();
        var sprites = new Sprite[spritesPath.Length];
        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        var accumulatedTime = 0f;
        for (int i = 0; i < spritesPath.Length; i++)
        {
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritesPath[i]);
            keyframes[i] = new ObjectReferenceKeyframe()
            {
                time = accumulatedTime,
                value = sprites[i]
            };
            accumulatedTime += times[i];
        }
        var clipPath = Path.Combine(folder, $"{name}.anim");
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            AssetDatabase.CreateAsset(new AnimationClip() { frameRate = 60 }, clipPath);
            clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        }
        
        AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        }, keyframes);

        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets(); 
        AssetDatabase.Refresh(); 
    }
    public static bool TryParseGif(this dImage img, out Bitmap[] maps, out float[] times)
    {
        maps = new Bitmap[0];
        times = new float[0];
        var ByteTimes = img.GetPropertyItem(0x5100).Value;
        try
        {
            var dimension = new FrameDimension(img.FrameDimensionsList[0]);
            var count = img.GetFrameCount(dimension);
            if (count > 1)
            {
                maps = new Bitmap[count];
                times = new float[count];
                for (int i = 0; i < count; i++)
                {
                    img.SelectActiveFrame(dimension, i);
                    times[i] = BitConverter.ToInt32(ByteTimes, i * 4) * 0.01f;
                    maps[i] = new Bitmap(img);
                }
                return true;
            }
        }
        catch (Exception e)
        {
            throw e;
        }
        return false;
    }
}
