using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates visual world-space decorations: medieval stone border, earth background,
/// path direction arrows, entry/base markers.
/// </summary>
public static class SceneDecorator
{
    public static void Build(GridManager grid, List<Vector3> waypoints)
    {
        AddBackground(grid);
        AddMedievalBorder(grid);
        AddPathArrows(waypoints);
        AddEntryExitMarkers(grid, waypoints);
    }

    // ── Earthy background behind the entire grid ──────────────────────
    private static void AddBackground(GridManager grid)
    {
        var go = new GameObject("Background");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = CreateEarthTexture(256);
        sr.sortingOrder = -10;

        float w = grid.Columns * grid.CellSize + 1.2f;
        float h = grid.Rows    * grid.CellSize + 1.2f;
        go.transform.localScale = new Vector3(w, h, 1f);
        go.transform.position   = new Vector3(
            grid.Origin.x + grid.Columns * grid.CellSize * 0.5f,
            grid.Origin.y + grid.Rows    * grid.CellSize * 0.5f, 1f);
    }

    // ── Medieval stone-wall crenellated border ────────────────────────
    private static void AddMedievalBorder(GridManager grid)
    {
        float cx = grid.Origin.x + grid.Columns * grid.CellSize * 0.5f;
        float cy = grid.Origin.y + grid.Rows    * grid.CellSize * 0.5f;
        float w  = grid.Columns * grid.CellSize;
        float h  = grid.Rows    * grid.CellSize;
        float t  = 0.28f; // border wall thickness

        Color wallColor   = new Color(0.42f, 0.37f, 0.28f);
        Color mortar      = new Color(0.30f, 0.26f, 0.20f);

        // Main wall strips
        CreateWallStrip("Wall_Top",    cx,             cy + h * 0.5f + t * 0.5f, w + t * 2, t, wallColor, mortar);
        CreateWallStrip("Wall_Bottom", cx,             cy - h * 0.5f - t * 0.5f, w + t * 2, t, wallColor, mortar);
        CreateWallStrip("Wall_Left",   cx - w*0.5f - t*0.5f, cy, t, h, wallColor, mortar);
        CreateWallStrip("Wall_Right",  cx + w*0.5f + t*0.5f, cy, t, h, wallColor, mortar);

        // Corner towers (small squares at each corner)
        float cz = t * 1.6f;
        Color cornerColor = new Color(0.36f, 0.32f, 0.24f);
        CreateCornerTower("Corner_TL", cx - w*0.5f - t*0.5f, cy + h*0.5f + t*0.5f, cz, cornerColor);
        CreateCornerTower("Corner_TR", cx + w*0.5f + t*0.5f, cy + h*0.5f + t*0.5f, cz, cornerColor);
        CreateCornerTower("Corner_BL", cx - w*0.5f - t*0.5f, cy - h*0.5f - t*0.5f, cz, cornerColor);
        CreateCornerTower("Corner_BR", cx + w*0.5f + t*0.5f, cy - h*0.5f - t*0.5f, cz, cornerColor);

        // Battlements (crenellations) on top wall
        float battleW = 0.22f, battleH = 0.22f, battleGap = 0.44f;
        float startX  = cx - w * 0.5f + battleGap * 0.5f;
        float battleY = cy + h * 0.5f + t + battleH * 0.5f;
        int numBattlements = Mathf.FloorToInt(w / battleGap);
        for (int i = 0; i < numBattlements; i++)
        {
            if (i % 2 == 0) continue; // skip every other for gap
            float bx = startX + i * battleGap;
            var bgo = new GameObject("Battlement");
            var sr  = bgo.AddComponent<SpriteRenderer>();
            sr.sprite = CreateStoneSprite(cornerColor, new Color(0.24f, 0.21f, 0.16f));
            sr.sortingOrder = -2;
            bgo.transform.position   = new Vector3(bx, battleY, 0f);
            bgo.transform.localScale = new Vector3(battleW, battleH, 1f);
        }
    }

    private static void CreateWallStrip(string name, float x, float y, float w, float h,
        Color stone, Color mortar)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateStoneWallTexture(stone, mortar, (int)(w * 64), Mathf.Max(1,(int)(h * 64)));
        sr.sortingOrder = -2;
        go.transform.position   = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);
    }

    private static void CreateCornerTower(string name, float x, float y, float size, Color col)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateStoneSprite(col, new Color(col.r * 0.7f, col.g * 0.7f, col.b * 0.7f));
        sr.sortingOrder = -1;
        go.transform.position   = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(size, size, 1f);
    }

    // ── Small triangular arrows along the path ───────────────────────
    private static void AddPathArrows(List<Vector3> waypoints)
    {
        if (waypoints == null || waypoints.Count < 2) return;
        Color arrowColor = new Color(0.90f, 0.78f, 0.42f, 0.70f);

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 from = waypoints[i];
            Vector3 to   = waypoints[i + 1];
            Vector3 dir  = (to - from).normalized;
            float   dist = Vector3.Distance(from, to);

            int count = Mathf.Max(1, Mathf.FloorToInt(dist / 1.5f));
            for (int k = 1; k <= count; k++)
            {
                float   t   = k / (float)(count + 1);
                Vector3 pos = Vector3.Lerp(from, to, t);
                pos.z = 0f;

                var go = new GameObject("PathArrow");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite       = CreateArrowSprite(arrowColor);
                sr.sortingOrder = 0;
                go.transform.position   = pos;
                go.transform.localScale = Vector3.one * 0.32f;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }

    // ── Entry (IN) and Base (OUT) medieval flag markers ──────────────
    private static void AddEntryExitMarkers(GridManager grid, List<Vector3> waypoints)
    {
        if (waypoints == null || waypoints.Count < 2) return;
        CreateMedievalFlag("Flag_IN",   waypoints[0],                   new Color(0.15f, 0.78f, 0.25f), "IN");
        CreateMedievalFlag("Flag_BASE", waypoints[waypoints.Count - 1], new Color(0.88f, 0.18f, 0.18f), "BASE");
    }

    /// <summary>
    /// Medieval flag-on-pole marker: stone base → wooden pole → coloured pennant with label.
    /// </summary>
    private static void CreateMedievalFlag(string name, Vector3 pos, Color flagCol, string label)
    {
        var root = new GameObject(name);
        root.transform.position = pos;

        // ── Stone/brick base ─────────────────────────────────────────
        var baseGo = new GameObject("Base");
        baseGo.transform.SetParent(root.transform);
        baseGo.transform.localPosition = new Vector3(0f, -0.30f, 0f);
        var baseSR = baseGo.AddComponent<SpriteRenderer>();
        baseSR.sprite       = SpriteFactory.CreateRoundedSquare(
            new Color(0.44f, 0.38f, 0.28f), new Color(0.28f, 0.24f, 0.18f), 32, 3);
        baseSR.sortingOrder = 3;
        baseGo.transform.localScale = new Vector3(0.42f, 0.22f, 1f);

        // ── Wooden pole ──────────────────────────────────────────────
        var poleGo = new GameObject("Pole");
        poleGo.transform.SetParent(root.transform);
        poleGo.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        var poleSR = poleGo.AddComponent<SpriteRenderer>();
        poleSR.sprite       = SpriteFactory.CreateSquare(new Color(0.52f, 0.33f, 0.14f));
        poleSR.sortingOrder = 3;
        poleGo.transform.localScale = new Vector3(0.10f, 1.50f, 1f);

        // Pole tip knob
        var knob = new GameObject("Knob");
        knob.transform.SetParent(root.transform);
        knob.transform.localPosition = new Vector3(0f, 1.26f, 0f);
        var knobSR = knob.AddComponent<SpriteRenderer>();
        knobSR.sprite       = SpriteFactory.CreateCircle(new Color(0.72f, 0.56f, 0.22f), new Color(0.48f, 0.36f, 0.12f), 32);
        knobSR.sortingOrder = 4;
        knob.transform.localScale = Vector3.one * 0.18f;

        // ── Pennant flag ─────────────────────────────────────────────
        var flagGo = new GameObject("Flag");
        flagGo.transform.SetParent(root.transform);
        // offset right so the left edge of the flag touches the pole
        flagGo.transform.localPosition = new Vector3(0.28f, 0.95f, 0f);
        var flagSR = flagGo.AddComponent<SpriteRenderer>();
        flagSR.sprite       = CreatePennantSprite(flagCol);
        flagSR.sortingOrder = 4;
        flagGo.transform.localScale = Vector3.one * 0.65f;

        // Flag ripple highlight strip
        var stripGo = new GameObject("Strip");
        stripGo.transform.SetParent(flagGo.transform);
        stripGo.transform.localPosition = new Vector3(-0.18f, 0f, 0f);
        var stripSR = stripGo.AddComponent<SpriteRenderer>();
        stripSR.sprite       = SpriteFactory.CreateSquare(new Color(1f, 1f, 1f, 0.18f));
        stripSR.sortingOrder = 5;
        stripGo.transform.localScale = new Vector3(0.12f, 0.72f, 1f);

        // ── Label on the flag ────────────────────────────────────────
        var lblGo = new GameObject("Label");
        lblGo.transform.SetParent(flagGo.transform);
        lblGo.transform.localPosition = new Vector3(0f, 0f, 0f);
        var tm = lblGo.AddComponent<TextMesh>();
        tm.text      = label;
        tm.fontSize  = 26;
        tm.color     = Color.white;
        tm.fontStyle = FontStyle.Bold;
        tm.anchor    = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        lblGo.transform.localScale = Vector3.one * 0.085f;
    }

    /// <summary>Creates a pennant sprite: rectangle with a V-notch cut into the right side.</summary>
    private static Sprite CreatePennantSprite(Color col)
    {
        const int W = 52, H = 34;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        var pix = new Color[W * H];
        Color dark = Color.Lerp(col, Color.black, 0.35f);
        Color lite = Color.Lerp(col, Color.white, 0.22f);

        for (int y = 0; y < H; y++)
        for (int x = 0; x < W; x++)
        {
            float nx = x / (float)W;
            float ny = Mathf.Abs(y / (float)H - 0.5f) * 2f;  // 0=centre, 1=top/bottom edge

            // V-notch from right: cut away where nx > threshold and the pixel is above the diagonal
            bool inNotch = nx > 0.60f && nx > (1f - ny * 0.45f);
            bool inside  = !inNotch && x > 0 && x < W - 1 && y > 1 && y < H - 2;

            if (!inside) { pix[y * W + x] = Color.clear; continue; }

            bool isEdge = x <= 1 || y <= 2 || y >= H - 3;
            float shade  = 1f - nx * 0.15f;   // slight left-to-right darkening (depth)
            Color base2  = Color.Lerp(col, lite, (1f - nx) * 0.3f);
            pix[y * W + x] = isEdge ? dark : new Color(base2.r * shade, base2.g * shade, base2.b * shade, 1f);
        }

        tex.SetPixels(pix);
        tex.Apply();
        // Pivot at the left-centre edge so the flag attaches to the pole
        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.05f, 0.5f), W);
    }

    // ── Procedural texture helpers ────────────────────────────────────

    private static Sprite CreateEarthTexture(int size)
    {
        var tex  = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pix  = new Color[size * size];
        var rng  = new System.Random(77);
        Color c1 = new Color(0.19f, 0.14f, 0.09f);
        Color c2 = new Color(0.26f, 0.20f, 0.13f);
        for (int i = 0; i < pix.Length; i++)
            pix[i] = Color.Lerp(c1, c2, (float)rng.NextDouble());
        tex.SetPixels(pix); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateStoneWallTexture(Color stone, Color mortar, int w, int h)
    {
        w = Mathf.Max(4, w); h = Mathf.Max(4, h);
        var tex = new Texture2D(w, h) { filterMode = FilterMode.Bilinear };
        var pix = new Color[w * h];
        var rng = new System.Random(42);

        int brickH = Mathf.Max(2, h / 3);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int row = y / brickH;
            int offset = (row % 2 == 0) ? 0 : w / 4;
            int brickW = Mathf.Max(4, w / 3);
            int bx = (x + offset) % brickW;

            bool isMortar = bx == 0 || y % brickH == 0;
            float vary = (float)(rng.NextDouble() * 0.15);
            Color c = isMortar ? mortar : Color.Lerp(stone, stone + new Color(vary, vary, vary, 0), 1f);
            pix[y * w + x] = c;
        }
        tex.SetPixels(pix); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), Mathf.Max(w, h));
    }

    private static Sprite CreateStoneSprite(Color fill, Color outline)
        => SpriteFactory.CreateRoundedSquare(fill, outline, 32, 3);

    private static Sprite CreateArrowSprite(Color col)
    {
        int size = 32;
        var tex  = new Texture2D(size, size) { filterMode = FilterMode.Bilinear };
        var pix  = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float nx = x / (float)size;
            float ny = Mathf.Abs(y / (float)size - 0.5f) * 2f;
            pix[y * size + x] = (nx > ny) ? col : Color.clear;
        }
        tex.SetPixels(pix); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
