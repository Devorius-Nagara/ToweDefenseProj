using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Attach to any empty GameObject in SampleScene.
/// Press Play — the entire game world is built in code, level-aware.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        // ── Ensure LevelManager exists (survives scene reloads) ───────
        if (LevelManager.Instance == null)
        {
            var lmGo = new GameObject("LevelManager");
            lmGo.AddComponent<LevelManager>();
        }
        var lvl = LevelManager.Instance.Current;

        // ── Camera ───────────────────────────────────────────────────
        var cam = Camera.main;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor  = new Color(0.08f, 0.12f, 0.16f);

        // ── Waypoints ─────────────────────────────────────────────────
        var pathGo = new GameObject("WaypointPath");
        var waypointPath = pathGo.AddComponent<WaypointPath>();
        var waypointList = new List<Vector3>();
        var origin = new Vector2(-lvl.GridCols * 0.5f, -lvl.GridRows * 0.5f);
        foreach (var c in lvl.PathCorners)
            waypointList.Add(GridToWorld(c.x, c.y, origin));
        waypointPath.SetWaypoints(waypointList);

        var pathCells = BuildPathCells(lvl.PathCorners);

        // ── Grid ──────────────────────────────────────────────────────
        var gridGo = new GameObject("GridManager");
        var grid   = gridGo.AddComponent<GridManager>();
        grid.Columns  = lvl.GridCols;
        grid.Rows     = lvl.GridRows;
        grid.CellSize = 1f;
        grid.Origin   = origin;
        grid.Initialize(pathCells);

        // ── Scene decoration ──────────────────────────────────────────
        SceneDecorator.Build(grid, waypointList);  // markers built inside SceneDecorator

        // ── Economy ───────────────────────────────────────────────────
        new GameObject("EconomyManager").AddComponent<EconomyManager>();

        // ── Pool ──────────────────────────────────────────────────────
        new GameObject("PoolManager").AddComponent<PoolManager>();

        // ── Audio ─────────────────────────────────────────────────────
        var audioGo  = new GameObject("AudioManager");
        var audio    = audioGo.AddComponent<AudioManager>();
        audio.SFXVolume = PlayerPrefs.GetFloat("sfx_vol", 0.7f);

        // ── Statistics ────────────────────────────────────────────────
        new GameObject("StatisticsManager").AddComponent<StatisticsManager>();

        // ── Enemy data ────────────────────────────────────────────────
        // Gold rewards increased by 25%
        var goblin = MakeEnemy("Goblin", hp:  40f, speed: 2.5f, gold:  6, cost: 10, dmg: 1, immune: false, color: new Color(0.30f, 0.80f, 0.20f));
        var orc    = MakeEnemy("Orc",    hp: 150f, speed: 1.2f, gold: 15, cost: 25, dmg: 3, immune: false, color: new Color(0.50f, 0.30f, 0.75f));
        var ghost  = MakeEnemy("Ghost",  hp:  80f, speed: 1.8f, gold: 13, cost: 20, dmg: 2, immune: true,  color: new Color(0.75f, 0.90f, 1.00f));
        var enemyTypes = new[] { goblin, orc, ghost };

        // ── Tower data ────────────────────────────────────────────────
        var archer  = MakeTower("Archer",  cost: 100, cd: 1.0f, range: 2.5f, dmg:  20f, aoe: false,                   slow: false,                          pSpeed:  9f, color: new Color(0.20f, 0.70f, 0.90f), desc: "Single target\nMedium range");
        var mage    = MakeTower("Mage",    cost: 150, cd: 2.2f, range: 2.0f, dmg:  35f, aoe: true,  aoeR: 1.2f,       slow: false,                          pSpeed:  7f, color: new Color(0.70f, 0.20f, 0.90f), desc: "AoE damage\nShort range");
        var freezer = MakeTower("Freezer", cost: 120, cd: 1.5f, range: 2.5f, dmg:  10f, aoe: false,                   slow: true, slowF: 0.40f, slowD: 2.5f, pSpeed:  8f, color: new Color(0.30f, 0.80f, 1.00f), desc: "Slows enemies\nIgnored by Ghosts");
        var cannon  = MakeTower("Cannon",  cost: 200, cd: 3.0f, range: 3.5f, dmg:  70f, aoe: false,                   slow: false,                          pSpeed: 12f, color: new Color(0.90f, 0.50f, 0.10f), desc: "Heavy damage\nLong range");
        var towerTypes = new List<TowerData> { archer, mage, freezer, cannon };

        // ── Object pools ──────────────────────────────────────────────
        var pm = PoolManager.Instance;
        foreach (var ed in enemyTypes)
        {
            var proto = CreateEnemyProto(ed);
            pm.CreatePool("enemy_" + ed.enemyName, proto, 15);
            proto.SetActive(false);
        }
        var projProto = CreateProjectileProto();
        pm.CreatePool("projectile", projProto, 40);
        projProto.SetActive(false);

        var mageProjProto = CreateMageProjectileProto();
        pm.CreatePool("mage_projectile", mageProjProto, 20);
        mageProjProto.SetActive(false);

        var arrowProto = CreateCustomProjectileProto("Sprites/arrow_proj", 96f,
            scale: 0.55f, faceTarget: true, rotSpeed: 0f);
        pm.CreatePool("archer_projectile", arrowProto, 30);
        arrowProto.SetActive(false);

        var iceProjProto = CreateCustomProjectileProto("Sprites/ice_proj", 64f,
            scale: 0.45f, faceTarget: false, rotSpeed: 120f);
        pm.CreatePool("freezer_projectile", iceProjProto, 20);
        iceProjProto.SetActive(false);

        var cannonballProto = CreateCustomProjectileProto("Sprites/cannonball_proj", 64f,
            scale: 0.38f, faceTarget: false, rotSpeed: 180f);
        pm.CreatePool("cannon_projectile", cannonballProto, 20);
        cannonballProto.SetActive(false);

        // ── Gameplay systems ──────────────────────────────────────────
        new GameObject("EnemySpawner").AddComponent<EnemySpawner>();

        var placerGo = new GameObject("TowerPlacer");
        var placer   = placerGo.AddComponent<TowerPlacer>();
        placer.AvailableTowers.AddRange(towerTypes);

        var aiGo = new GameObject("AIWaveBuilder");
        var ai   = aiGo.AddComponent<AIWaveBuilder>();
        ai.SetEnemyTypes(enemyTypes);

        new GameObject("GameManager").AddComponent<GameManager>();

        // ── UI (built one frame later so all Awake calls finish first) ─
        var uiGo = new GameObject("UIManager");
        var ui   = uiGo.AddComponent<UIManager>();
        StartCoroutine(BuildUINextFrame(ui, towerTypes));
    }

    private IEnumerator BuildUINextFrame(UIManager ui, List<TowerData> towers)
    {
        yield return null;
        ui.BuildUI(towers);
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static Vector3 GridToWorld(int col, int row, Vector2 origin)
        => new(origin.x + col + 0.5f, origin.y + row + 0.5f, 0f);

    private static HashSet<Vector2Int> BuildPathCells(Vector2Int[] corners)
    {
        var cells = new HashSet<Vector2Int>();
        for (int i = 0; i < corners.Length - 1; i++)
        {
            var a = corners[i]; var b = corners[i + 1];
            int dc = System.Math.Sign(b.x - a.x);
            int dr = System.Math.Sign(b.y - a.y);
            var cur = a;
            int limit = 50, steps = 0;
            cells.Add(cur);
            while (cur != b && steps++ < limit)
            { cur += new Vector2Int(dc, dr); cells.Add(cur); }
        }
        return cells;
    }

    private void CreateMarker(string label, Vector3 pos, Color color)
    {
        var go = new GameObject("Marker_" + label);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateCircle(color, Color.white, 64);
        sr.sortingOrder = 3;
        go.transform.localScale = Vector3.one * 0.55f;

        var nm = new GameObject("Lbl");
        nm.transform.SetParent(go.transform);
        nm.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        var tm = nm.AddComponent<TextMesh>();
        tm.text = label; tm.fontSize = 18; tm.color = color;
        tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
        nm.transform.localScale = Vector3.one * 0.14f;
    }

    private GameObject CreateEnemyProto(EnemyData data)
    {
        var go = new GameObject("EnemyProto_" + data.enemyName);
        go.transform.SetParent(transform);

        go.AddComponent<SpriteRenderer>().sprite =
            SpriteFactory.CreateCircle(data.enemyColor, Color.Lerp(data.enemyColor, Color.black, 0.5f), 64);
        go.GetComponent<SpriteRenderer>().sortingOrder = 5;
        go.transform.localScale = Vector3.one * 0.6f;
        go.AddComponent<EnemyController>();

        Child(go, "HPBar_BG", new Vector3(0f, 0.75f, 0f), new Vector3(0.9f, 0.12f, 1f),
              SpriteFactory.CreateSquare(new Color(0.8f, 0.1f, 0.1f)), 6);
        Child(go, "HPBar_FG", new Vector3(0f, 0.75f, 0f), new Vector3(0.9f, 0.12f, 1f),
              SpriteFactory.CreateSquare(new Color(0.1f, 0.8f, 0.1f)), 7);

        var nm = new GameObject("Nm");
        nm.transform.SetParent(go.transform);
        nm.transform.localPosition = new Vector3(0f, -0.85f, 0f);
        var tm = nm.AddComponent<TextMesh>();
        tm.text = data.enemyName[0].ToString(); tm.fontSize = 18; tm.color = Color.white;
        tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
        nm.transform.localScale = Vector3.one * 0.12f;

        return go;
    }

    private GameObject CreateProjectileProto()
    {
        var go = new GameObject("ProjProto");
        go.transform.SetParent(transform);
        go.AddComponent<SpriteRenderer>().sprite = SpriteFactory.CreateCircle(Color.yellow, Color.white, 32);
        go.GetComponent<SpriteRenderer>().sortingOrder = 8;
        go.transform.localScale = Vector3.one * 0.18f;
        go.AddComponent<Projectile>();
        return go;
    }

    private GameObject CreateMageProjectileProto()
    {
        var go = new GameObject("MageProjProto");
        go.transform.SetParent(transform);
        var sr = go.AddComponent<SpriteRenderer>();

        var fireSprite = SpriteLoader.Load("Sprites/magfiresplash");
        sr.sprite = fireSprite != null
            ? fireSprite
            : SpriteFactory.CreateCircle(new Color(1f, 0.35f, 0f), new Color(1f, 0.8f, 0f), 32);

        sr.sortingOrder = 8;
        // Scale so the fire is ~0.4 units — visible but not blocking the view
        go.transform.localScale = Vector3.one * 0.40f;

        var proj = go.AddComponent<Projectile>();
        proj.rotationSpeed = 90f;   // slow spin while flying
        return go;
    }

    private GameObject CreateCustomProjectileProto(string texPath, float ppu,
        float scale, bool faceTarget, float rotSpeed)
    {
        var go = new GameObject("ProjProto_" + System.IO.Path.GetFileNameWithoutExtension(texPath));
        go.transform.SetParent(transform);
        var sr     = go.AddComponent<SpriteRenderer>();
        var loaded = SpriteLoader.Load(texPath, ppu);
        sr.sprite  = loaded != null
            ? loaded
            : SpriteFactory.CreateCircle(Color.white, Color.gray, 32);
        sr.sortingOrder = 8;
        go.transform.localScale = Vector3.one * scale;
        var proj = go.AddComponent<Projectile>();
        proj.faceTarget   = faceTarget;
        proj.rotationSpeed = rotSpeed;
        return go;
    }

    private static void Child(GameObject parent, string name, Vector3 lpos, Vector3 lscale, Sprite sprite, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = lpos;
        go.transform.localScale    = lscale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite; sr.sortingOrder = order;
    }

    // ── ScriptableObject factories ─────────────────────────────────────

    private EnemyData MakeEnemy(string nm, float hp, float speed, int gold,
        int cost, int dmg, bool immune, Color color)
    {
        var d = ScriptableObject.CreateInstance<EnemyData>();
        d.enemyName = nm; d.maxHealth = hp; d.moveSpeed = speed;
        d.goldReward = gold; d.waveCost = cost; d.baseDamage = dmg;
        d.immuneToSlow = immune; d.enemyColor = color;
        return d;
    }

    private TowerData MakeTower(string nm, int cost, float cd, float range, float dmg,
        bool aoe, bool slow, float pSpeed, Color color, string desc,
        float aoeR = 0f, float slowF = 0.5f, float slowD = 2f)
    {
        var d = ScriptableObject.CreateInstance<TowerData>();
        d.towerName = nm; d.cost = cost; d.attackCooldown = cd; d.range = range; d.damage = dmg;
        d.isAoE = aoe; d.aoeRadius = aoeR; d.isSlowing = slow; d.slowFactor = slowF; d.slowDuration = slowD;
        d.projectileSpeed = pSpeed; d.towerColor = color; d.description = desc;
        return d;
    }
}
