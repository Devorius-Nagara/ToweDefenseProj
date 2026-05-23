using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Click-to-place, LMB-on-placed to upgrade, RMB-on-placed to sell.
/// Works during BOTH Preparation AND Battle phases.
/// </summary>
public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Instance { get; private set; }

    private TowerData selectedTower;
    private int lastHoverCol = -1, lastHoverRow = -1;

    public List<TowerData> AvailableTowers { get; } = new();

    // Stores placed TowerControllers by cell; data accessible via ctrl.Data
    private readonly Dictionary<Vector2Int, TowerController> placedTowers = new();

    void Awake() => Instance = this;

    void Update()
    {
        var state = GameManager.Instance.State;
        if (state != GameState.Preparation && state != GameState.Battle) return;

        bool lmb = Mouse.current.leftButton.wasPressedThisFrame;
        bool rmb = Mouse.current.rightButton.wasPressedThisFrame;

        if (selectedTower != null)
        {
            UpdateHover();
            if (lmb) TryPlace();
            if (rmb) Deselect();
        }
        else
        {
            if (rmb) TrySell();
            if (lmb) TryUpgradeClick();
        }
    }

    private void UpdateHover()
    {
        Vector3 world = MouseWorld();
        if (GridManager.Instance.WorldToCell(world, out int col, out int row))
        {
            if (col != lastHoverCol || row != lastHoverRow)
            {
                GridManager.Instance.ClearHover(lastHoverCol, lastHoverRow);
                bool ok = GridManager.Instance.CanPlaceTower(col, row)
                       && EconomyManager.Instance.Gold >= selectedTower.cost;
                GridManager.Instance.SetHover(col, row, ok);
                lastHoverCol = col; lastHoverRow = row;
            }
        }
        else
        {
            GridManager.Instance.ClearHover(lastHoverCol, lastHoverRow);
            lastHoverCol = lastHoverRow = -1;
        }
    }

    private void TryPlace()
    {
        Vector3 world = MouseWorld();
        if (!GridManager.Instance.WorldToCell(world, out int col, out int row)) return;
        if (!GridManager.Instance.CanPlaceTower(col, row)) return;
        if (!EconomyManager.Instance.SpendGold(selectedTower.cost)) return;

        var go   = CreateTowerObject(selectedTower);
        var ctrl = go.GetComponent<TowerController>();
        GridManager.Instance.PlaceTower(col, row, go);
        placedTowers[new Vector2Int(col, row)] = ctrl;

        AudioManager.Instance?.PlayPlace();
        StatisticsManager.Instance?.OnTowerPlaced();

        GridManager.Instance.ClearHover(lastHoverCol, lastHoverRow);
        lastHoverCol = lastHoverRow = -1;
    }

    private void TrySell()
    {
        Vector3 world = MouseWorld();
        if (!GridManager.Instance.WorldToCell(world, out int col, out int row)) return;
        SellAt(col, row);
    }

    private void TryUpgradeClick()
    {
        Vector3 world = MouseWorld();
        if (!GridManager.Instance.WorldToCell(world, out int col, out int row)) return;

        var key = new Vector2Int(col, row);
        if (!placedTowers.TryGetValue(key, out var ctrl)) return;

        if (!ctrl.CanUpgrade)
        {
            UIManager.Instance?.SetStatusText($"{ctrl.Data.towerName} is already at max level!");
            return;
        }

        int cost = ctrl.UpgradeCost;
        if (!EconomyManager.Instance.SpendGold(cost))
        {
            UIManager.Instance?.SetStatusText($"Not enough gold! Upgrade costs {cost}g");
            return;
        }

        ctrl.Upgrade();
        AudioManager.Instance?.PlayPlace();
        UIManager.Instance?.SetStatusText(
            $"Upgraded {ctrl.Data.towerName} to level {ctrl.UpgradeLevel + 1}!  (+range +speed)");
    }

    private void SellAt(int col, int row)
    {
        var key = new Vector2Int(col, row);
        if (!placedTowers.TryGetValue(key, out var ctrl)) return;

        int refund = Mathf.RoundToInt(ctrl.Data.cost * 0.5f);
        GridManager.Instance.RemoveTower(col, row);
        placedTowers.Remove(key);
        EconomyManager.Instance.AddGold(refund);
        AudioManager.Instance?.PlaySell();
        StatisticsManager.Instance?.OnTowerSold();
        UIManager.Instance?.SetStatusText($"Sold {ctrl.Data.towerName} — refunded {refund}g");
    }

    private Vector3 MouseWorld()
    {
        Vector2 screen = Mouse.current.position.ReadValue();
        var p = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
        p.z = 0f;
        return p;
    }

    private GameObject CreateTowerObject(TowerData data)
    {
        var go = new GameObject($"Tower_{data.towerName}");
        go.transform.SetParent(transform);

        Color fill    = data.towerColor;
        Color outline = Color.Lerp(data.towerColor, Color.black, 0.4f);

        Sprite towerSprite;
        float  towerScale;
        bool   useCustomSprite = false;

        // Map each tower name to its sprite resource path (fallback sprite if load fails)
        string spritePath = data.towerName switch
        {
            "Archer"  => "Sprites/archer_tower",
            "Mage"    => "Sprites/magtower",
            "Freezer" => "Sprites/freezer_tower",
            "Cannon"  => "Sprites/cannon_tower",
            _         => null
        };

        if (spritePath != null)
        {
            var sprite = SpriteLoader.Load(spritePath);
            if (sprite != null)
            {
                towerSprite     = sprite;
                towerScale      = 0.82f;
                useCustomSprite = true;
            }
            else
            {
                // Fallback to geometric shape
                towerSprite = data.towerName switch
                {
                    "Archer"  => SpriteFactory.CreateRoundedSquare(fill, outline, 32, 2),
                    "Mage"    => SpriteFactory.CreateCircle(fill, outline, 64),
                    "Freezer" => SpriteFactory.CreateDiamond(fill, outline),
                    "Cannon"  => SpriteFactory.CreateTriangle(fill, outline),
                    _         => SpriteFactory.CreateRoundedSquare(fill, outline)
                };
                towerScale = data.towerName == "Cannon" ? 0.85f : 0.72f;
            }
        }
        else
        {
            towerSprite = SpriteFactory.CreateRoundedSquare(fill, outline);
            towerScale  = 0.72f;
        }

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = towerSprite;
        sr.sortingOrder = 2;
        go.transform.localScale = Vector3.one * towerScale;

        go.AddComponent<BoxCollider2D>().size = Vector2.one;

        var ctrl = go.AddComponent<TowerController>();
        ctrl.Init(data);
        ctrl.MainRenderer = sr;   // wire up for flip-on-shoot

        // Each tower type uses its own projectile pool
        ctrl.ProjectilePoolKey = data.towerName switch
        {
            "Archer"  => "archer_projectile",
            "Mage"    => "mage_projectile",
            "Freezer" => "freezer_projectile",
            "Cannon"  => "cannon_projectile",
            _         => "projectile"
        };

        // Letter label — skip for custom-sprite Archer (it has its own art)
        if (!useCustomSprite)
        {
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform);
            labelGo.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = data.towerName[0].ToString();
            tm.fontSize = 28; tm.color = Color.white;
            tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
            labelGo.transform.localScale = Vector3.one * 0.10f;
        }

        return go;
    }

    public void SelectTower(TowerData data)
    {
        selectedTower = data;
        UIManager.Instance?.SetStatusText(
            $"Placing: {data.towerName} ({data.cost}g)  |  RMB to cancel  |  LMB placed tower: upgrade  |  RMB placed tower: sell (50%)");
    }

    public void Deselect()
    {
        selectedTower = null;
        GridManager.Instance.ClearHover(lastHoverCol, lastHoverRow);
        lastHoverCol = lastHoverRow = -1;
        UIManager.Instance?.SetStatusText("LMB placed tower: upgrade  |  RMB placed tower: sell (50% refund)");
    }

    public void ClearPlacedData() => placedTowers.Clear();
}
