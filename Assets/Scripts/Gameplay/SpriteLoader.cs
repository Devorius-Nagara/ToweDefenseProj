using UnityEngine;

/// <summary>
/// Loads sprites from Resources with a fallback to direct disk read (useful in Editor
/// before Unity has had a chance to import newly added files).
/// In WebGL builds only the first two methods run (no file-system access).
/// </summary>
public static class SpriteLoader
{
    /// <summary>
    /// Load a sprite from Assets/Resources/{path}.png.
    /// Tries Resources.Load{Sprite} → Resources.Load{Texture2D} → direct file read.
    /// Returns null if all methods fail.
    /// </summary>
    public static Sprite Load(string resourcesPath, float pixelsPerUnit = -1f)
    {
        // 1) Try loading as Sprite directly (works when Unity imported as Sprite type)
        var sprite = Resources.Load<Sprite>(resourcesPath);
        if (sprite != null) return sprite;

        // 2) Try loading underlying Texture2D (works for Texture2D import type)
        var tex = Resources.Load<Texture2D>(resourcesPath);
        if (tex != null)
        {
            float ppu = pixelsPerUnit > 0 ? pixelsPerUnit : tex.width;
            return Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), ppu);
        }

        // 3) In Editor only: load raw bytes from disk before Unity imports the file.
        //    This block is completely stripped from WebGL / standalone builds.
#if UNITY_EDITOR
        string fullPath = Application.dataPath + "/Resources/" + resourcesPath + ".png";
        if (System.IO.File.Exists(fullPath))
        {
            byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
            var tex2 = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Bilinear };
            if (tex2.LoadImage(bytes))
            {
                float ppu2 = pixelsPerUnit > 0 ? pixelsPerUnit : tex2.width;
                return Sprite.Create(tex2,
                    new Rect(0, 0, tex2.width, tex2.height),
                    new Vector2(0.5f, 0.5f), ppu2);
            }
        }
#endif
        return null;
    }
}
