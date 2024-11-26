namespace GifUtility
{
    using System;
    using dImage = System.Drawing.Image;
    using Bitmap = System.Drawing.Bitmap;
    using Rectangle = System.Drawing.Rectangle;
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using System.Linq;
    using Yu5h1Lib.Winform;
    using System.Collections.Generic;

    public static class TextureImporterEx
    {
        //public static Dictionary<string, Rectangle[]> boundsCache = new Dictionary<string, Rectangle[]>();

        public static bool IsGif(this MenuCommand command)
            => ((TextureImporter)command.context).assetPath.EndsWith("gif", StringComparison.OrdinalIgnoreCase);

        #region MenuItem
        public const string LabelBase = "CONTEXT/TextureImporter/";
        public const string LabelExtractAnimClip = LabelBase + "Gif/Extract/AnimClip";
        public const string LabelAnimClip_X = LabelExtractAnimClip + " X";
        public const string LabelAnimClip_Y = LabelExtractAnimClip + " Y";
        public const string LabelMotionAnimClip = LabelExtractAnimClip + " Motion";

        [MenuItem(LabelExtractAnimClip, true)]
        private static bool ValidateItemBase(MenuCommand command) => command.IsGif();
        [MenuItem(LabelAnimClip_X, true)]
        private static bool ValidateItemX(MenuCommand command) => command.IsGif();
        [MenuItem(LabelAnimClip_Y, true)]
        private static bool ValidateItemY(MenuCommand command) => command.IsGif();
        [MenuItem(LabelMotionAnimClip, true)]
        private static bool ValidateItemMotion(MenuCommand command) => command.IsGif();

        [MenuItem(LabelExtractAnimClip)]
        public static void Gif2AnimClip(MenuCommand command) => command.RunGif2AnimClip();
        [MenuItem(LabelAnimClip_X)]
        public static void Gif2MotionAnimClipX(MenuCommand command) => command.RunGif2AnimClip(Vector2.right);
        [MenuItem(LabelAnimClip_Y)]
        public static void Gif2MotionAnimClipY(MenuCommand command) => command.RunGif2AnimClip(Vector2.up);
        [MenuItem(LabelMotionAnimClip)]
        public static void Gif2MotionAnimClip(MenuCommand command) => command.RunGif2AnimClip(Vector2.one);

        #endregion

 

        public static void RunGif2AnimClip(this MenuCommand command, Vector2? movementMultiplier = null)
        {
            var importer = (TextureImporter)command.context;
            if (!importer.assetPath.EndsWith("gif", StringComparison.OrdinalIgnoreCase))
                return;
            
            bool crop = movementMultiplier != null;
            var multiplier = crop ? movementMultiplier.Value : Vector2.one;

            var assetPath = importer.assetPath;
            var folder = Path.GetDirectoryName(assetPath);
            var name = Path.GetFileNameWithoutExtension(assetPath);
            var gif = dImage.FromFile(assetPath);
            
            if (!gif.TryExtraGif(out Bitmap[] maps, out float[] times,out Rectangle[] bounds))
            {
                gif.Dispose();
                return;
            }
            var positions = new Vector2[maps.Length];

            var spritesPath = maps.Select((map, i) => Path.Combine(folder, $"{name}{i:00}.png")).ToArray();

            for (int i = 0; i < maps.Length; i++)
            {
                var map = maps[i];
                if (crop)
                {
                    map = maps[i].Clone(bounds[i], maps[i].PixelFormat);
                    maps[i].Dispose();
                }
                map.Save(spritesPath[i]);
                map.Dispose();
            }

            gif.Dispose();
            AssetDatabase.Refresh();
            Sprite sprite = null;
            var spritekeyframes = new ObjectReferenceKeyframe[spritesPath.Length+1];
            var accumulatedTime = 0f;
            for (int i = 0; i < spritesPath.Length; i++)
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritesPath[i]);
                spritekeyframes[i] = new ObjectReferenceKeyframe()
                {
                    time = accumulatedTime,
                    value = sprite
                };
                accumulatedTime += times[i];
            }
            spritekeyframes[spritesPath.Length] = new ObjectReferenceKeyframe()
            {
                time = accumulatedTime,
                value = sprite
            };

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
            }, spritekeyframes);

            if (crop)
            {
                if (multiplier.x != 0)
                {
                    var posCurveX = new AnimationCurve();
                    posCurveX.AddKey(0, 0);
                    for (int i = 1; i < spritekeyframes.Length; i++)
                        posCurveX.AddKey(spritekeyframes[i].time, bounds[i].X / importer.spritePixelsPerUnit * multiplier.x);
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "Position.x"), posCurveX);
                }
                if (multiplier.y != 0)
                {
                    var posCurveY = new AnimationCurve();
                    posCurveY.AddKey(0, 0);
                    for (int i = 1; i < spritekeyframes.Length; i++)
                        posCurveY.AddKey(spritekeyframes[i].time, bounds[i].Y / importer.spritePixelsPerUnit * multiplier.y);
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "Position.y"), posCurveY);
                }
            }

            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        [MenuItem("CONTEXT/AnimationClip/Set Position Curve from Gif")]
        public static void SetPositionCurveFromGif(this MenuCommand command) 
            => LoadPositionFromGif(command, Vector2.one);

        public static void LoadPositionFromGif(this MenuCommand command, Vector2? movementMultiplier = null)
        {
            var clip = (AnimationClip)command.context;
            var assetPath = AssetDatabase.GetAssetPath(clip);
            var folder = new PathInfo(PathInfo.GetDirectory(assetPath));
            var gifName = PathInfo.GetName(assetPath) + ".gif";
            if ($"{gifName} was not found !".printWarningIf(!folder.ExistsAny(gifName)))
                return;
            var gifPath = folder.CombineWith(gifName);

            var gifImporter = AssetImporter.GetAtPath(gifPath) as TextureImporter;

            var gif = dImage.FromFile(gifPath);
            if (!gif.TryExtraGif(out Bitmap[] maps, out float[] times, out Rectangle[] bounds))
            {
                gif.Dispose();
                $"{gifName} parsing failed !".printWarning();
                return;
            }
  
            if (movementMultiplier == null)
                movementMultiplier = Vector2.one;

            var multiplier = movementMultiplier.Value;
            var accumulatedTimes = new float[times.Length+1];
            var sum = accumulatedTimes[0] = 0F;
            for (int i = 0; i < times.Length; i++)
            {
                sum += times[i];
                accumulatedTimes[i+1] = sum;
            }

            if (multiplier.x != 0)
            {
                var posCurveX = new AnimationCurve();
                posCurveX.AddKey(0, 0);
                for (int i = 1; i < accumulatedTimes.Length; i++)
                {
                    posCurveX.AddKey(accumulatedTimes[i], bounds[i].X / gifImporter.spritePixelsPerUnit * multiplier.x);
                    $"{times[i]} {bounds[i]}".print();
                }
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "Position.x"), posCurveX);
            }
            if (multiplier.y != 0)
            {
                var psCurveY = new AnimationCurve();
                psCurveY.AddKey(0, 0);
                for (int i = 1; i < accumulatedTimes.Length; i++)
                    psCurveY.AddKey(accumulatedTimes[i], bounds[i].Y / gifImporter.spritePixelsPerUnit * multiplier.y);
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "Position.y"), psCurveY);
            }
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            foreach (var map in maps)
                map.Dispose();
            AssetDatabase.Refresh();

        }
    } 
}
namespace Yu5h1Lib.Winform
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;

    public static class BitmapEx
    {
        public static bool TryExtraGif(this Image img, out Bitmap[] maps, out float[] times, out Rectangle[] bounds)
        {
            maps = new Bitmap[0];
            times = new float[0];
            bounds = new Rectangle[0];

            var ByteTimes = img.GetPropertyItem(0x5100).Value;
            try
            {
                var dimension = new FrameDimension(img.FrameDimensionsList[0]);
                var count = img.GetFrameCount(dimension);
                if (count > 1)
                {
                    maps = new Bitmap[count];
                    times = new float[count];
                    bounds = new Rectangle[count];

                    for (int i = 0; i < count; i++)
                    {
                        img.SelectActiveFrame(dimension, i);
                        times[i] = BitConverter.ToInt32(ByteTimes, i * 4) * 0.01f;
                        maps[i] = new Bitmap(img);
                        bounds[i] = maps[i].GetOpaquePixelBoundary();
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
        public static Rectangle GetOpaquePixelBoundary(this Bitmap bitmap)
        {
            int minX = bitmap.Width;
            int minY = bitmap.Height;
            int maxX = 0;
            int maxY = 0;

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    if (pixelColor.A > 0)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (minX < maxX && minY < maxY)
            {
                return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
            else
            {
                return Rectangle.Empty;
            }
        }
    }
}
