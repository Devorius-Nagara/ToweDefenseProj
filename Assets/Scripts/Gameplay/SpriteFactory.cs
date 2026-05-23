using UnityEngine;

/// <summary>Creates simple procedural sprites at runtime — no external assets needed.</summary>
public static class SpriteFactory
{
    public static Sprite CreateSquare(Color color, int size = 32)
    {
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateCircle(Color fill, Color border, int size = 64)
    {
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[size * size];
        float cx = size / 2f, cy = size / 2f, r = size / 2f - 1f, br = r - 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            if (d < br)        pixels[y * size + x] = fill;
            else if (d < r)    pixels[y * size + x] = border;
            else               pixels[y * size + x] = Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite CreateRing(Color ringColor, int size = 64, float thickness = 2f)
    {
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[size * size];
        float cx = size / 2f, cy = size / 2f, outer = size / 2f - 1f, inner = outer - thickness;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
            pixels[y * size + x] = (d >= inner && d <= outer) ? ringColor : Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    // ── Medieval tile textures ───────────────────────────────────────────

    /// <summary>Procedural grass tile — slightly varied green, good for empty grid cells.</summary>
    public static Sprite CreateMedievalGrass(int seed = 0, int size = 64)
    {
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[size * size];
        var rng = new System.Random(seed);

        // base green with variation
        Color baseC  = new Color(0.22f, 0.48f, 0.18f);
        Color darkC  = new Color(0.16f, 0.36f, 0.12f);
        Color lightC = new Color(0.30f, 0.58f, 0.22f);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = (float)(rng.NextDouble() * 0.35);
            Color c = Color.Lerp(darkC, lightC, n + 0.3f);

            // Subtle grid-like stone border at cell edges (inner frame)
            bool nearEdge = x < 3 || x >= size - 3 || y < 3 || y >= size - 3;
            if (nearEdge) c = Color.Lerp(c, new Color(0.18f, 0.38f, 0.14f), 0.4f);

            pixels[y * size + x] = c;
        }

        // Sparse grass blade hints
        for (int i = 0; i < 18; i++)
        {
            int bx = rng.Next(4, size - 4);
            int by = rng.Next(4, size - 4);
            Color blade = new Color(0.35f, 0.62f, 0.18f, 0.8f);
            for (int dy = 0; dy < 4; dy++)
                pixels[(by + dy) * size + bx] = Color.Lerp(pixels[(by + dy) * size + bx], blade, 0.6f);
        }

        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Procedural cobblestone tile — for path cells.</summary>
    public static Sprite CreateMedievalCobblestone(int seed = 0, int size = 64)
    {
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[size * size];
        var rng = new System.Random(seed + 999);

        Color stoneBase  = new Color(0.55f, 0.48f, 0.35f);
        Color stoneDark  = new Color(0.38f, 0.33f, 0.24f);
        Color stoneLight = new Color(0.68f, 0.60f, 0.44f);
        Color mortar     = new Color(0.32f, 0.28f, 0.22f);

        // Fill with stone noise
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float n = (float)(rng.NextDouble() * 0.4);
            pixels[y * size + x] = Color.Lerp(stoneDark, stoneLight, n + 0.3f);
        }

        // Draw 6 "cobblestones" per tile in a 2×3 grid with mortar lines
        int cols = 2, rows = 3;
        int cw = size / cols, ch = size / rows;
        for (int cr = 0; cr < rows; cr++)
        for (int cc = 0; cc < cols; cc++)
        {
            int ox = (int)(rng.NextDouble() * 3 - 1.5);
            int oy = (int)(rng.NextDouble() * 3 - 1.5);
            int x0 = cc * cw + 2 + ox, y0 = cr * ch + 2 + oy;
            int x1 = x0 + cw - 4,     y1 = y0 + ch - 4;
            x0 = Mathf.Clamp(x0, 0, size - 1); x1 = Mathf.Clamp(x1, 0, size - 1);
            y0 = Mathf.Clamp(y0, 0, size - 1); y1 = Mathf.Clamp(y1, 0, size - 1);

            float brightness = (float)(rng.NextDouble() * 0.25 + 0.55);
            Color stone = Color.Lerp(stoneDark, stoneLight, brightness);

            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                bool border = x == x0 || x == x1 || y == y0 || y == y1;
                pixels[y * size + x] = border ? mortar : stone;
            }
        }

        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Diamond (rhombus) shape — for Freezer tower.</summary>
    public static Sprite CreateDiamond(Color fill, Color outline, int size = 32)
    {
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[size * size];
        float cx = size / 2f, cy = size / 2f, r = size / 2f - 1f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Mathf.Abs(x - cx) + Mathf.Abs(y - cy);
            if      (d < r - 2f) pixels[y * size + x] = fill;
            else if (d < r)      pixels[y * size + x] = outline;
            else                 pixels[y * size + x] = Color.clear;
        }
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Upward-pointing triangle — for Cannon tower.</summary>
    public static Sprite CreateTriangle(Color fill, Color outline, int size = 32)
    {
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[size * size];
        float cx = size / 2f, top = size - 2f, bot = 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float t = (y - bot) / (top - bot);
            if (t < 0f || t > 1f) { pixels[y * size + x] = Color.clear; continue; }
            float hw = (1f - t) * (size / 2f - 1f);
            float edge = hw - Mathf.Abs(x - cx);
            if      (edge > 2f)  pixels[y * size + x] = fill;
            else if (edge >= 0f) pixels[y * size + x] = outline;
            else                 pixels[y * size + x] = Color.clear;
        }
        tex.SetPixels(pixels); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    /// <summary>Creates a rounded rectangle (tower body shape).</summary>
    public static Sprite CreateRoundedSquare(Color fill, Color outline, int size = 32, int radius = 4)
    {
        var tex = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float nx = Mathf.Clamp(x, radius, size - 1 - radius);
            float ny = Mathf.Clamp(y, radius, size - 1 - radius);
            float dx = x - nx, dy = y - ny;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            if (d > radius)         pixels[y * size + x] = Color.clear;
            else if (d > radius - 2) pixels[y * size + x] = outline;
            else                     pixels[y * size + x] = fill;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
