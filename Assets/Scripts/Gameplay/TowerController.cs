using UnityEngine;
using System.Collections.Generic;

public class TowerController : MonoBehaviour
{
    public TowerData Data { get; private set; }
    public TargetingMode Targeting = TargetingMode.FirstInPath;

    // Upgrade state
    public int  UpgradeLevel { get; private set; } = 0;   // 0 = base, max 2
    public bool CanUpgrade   => UpgradeLevel < 2;
    public int  UpgradeCost  => Mathf.RoundToInt(Data.cost * (0.5f + UpgradeLevel * 0.25f));

    private float cooldownTimer;
    private float currentRange;
    private float currentCooldown;
    /// <summary>Pool key for projectiles — override to use a custom projectile sprite.</summary>
    public string ProjectilePoolKey { get; set; } = "projectile";

    // Range indicator
    private SpriteRenderer rangeIndicator;

    // Upgrade level bars label
    private TextMesh upgradeLabel;

    /// <summary>Set by TowerPlacer — allows flipping the sprite on shoot direction.</summary>
    public SpriteRenderer MainRenderer { get; set; }

    public void Init(TowerData data)
    {
        Data             = data;
        currentRange     = data.range;
        currentCooldown  = data.attackCooldown;
        cooldownTimer    = data.attackCooldown;

        // Range indicator ring
        var ri = new GameObject("RangeIndicator");
        ri.transform.SetParent(transform);
        ri.transform.localPosition = Vector3.zero;
        ri.transform.localScale = Vector3.one * data.range * 2f;
        rangeIndicator = ri.AddComponent<SpriteRenderer>();
        rangeIndicator.sprite = SpriteFactory.CreateRing(new Color(1f, 1f, 0f, 0.35f), 128, 4f);
        rangeIndicator.sortingOrder = 10;
        rangeIndicator.enabled = false;

        // Upgrade level label (shows | per upgrade level)
        var lblGo = new GameObject("UpgradeLabel");
        lblGo.transform.SetParent(transform);
        lblGo.transform.localPosition = new Vector3(0.40f, 0.40f, 0f);
        upgradeLabel = lblGo.AddComponent<TextMesh>();
        upgradeLabel.fontSize  = 28;
        upgradeLabel.color     = Color.yellow;
        upgradeLabel.anchor    = TextAnchor.LowerLeft;
        upgradeLabel.alignment = TextAlignment.Left;
        lblGo.transform.localScale = Vector3.one * 0.09f;
        upgradeLabel.text = "";
    }

    void Update()
    {
        if (GameManager.Instance.State != GameState.Battle) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            EnemyController best = FindBestTarget();
            if (best != null)
            {
                Shoot(best);
                cooldownTimer = currentCooldown;
            }
        }
    }

    private EnemyController FindBestTarget()
    {
        EnemyController best = null;
        float bestScore = float.MinValue;

        foreach (var enemy in EnemySpawner.Instance.ActiveEnemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist > currentRange) continue;

            float score = Targeting switch
            {
                TargetingMode.FirstInPath  =>  enemy.PathProgress,
                TargetingMode.LastInPath   => -enemy.PathProgress,
                TargetingMode.LowestHP     => -dist,
                TargetingMode.HighestHP    =>  dist,
                _                          =>  enemy.PathProgress
            };

            if (score > bestScore) { bestScore = score; best = enemy; }
        }
        return best;
    }

    private void Shoot(EnemyController target)
    {
        // Flip sprite horizontally depending on target direction along X axis
        if (MainRenderer != null)
            MainRenderer.flipX = target.transform.position.x < transform.position.x;

        AudioManager.Instance?.PlayTowerShoot(Data.towerName);

        var go = PoolManager.Instance.Get(ProjectilePoolKey);
        go.transform.position = transform.position;

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Data.towerColor;

        var proj = go.GetComponent<Projectile>();
        proj.Launch(target, Data, ProjectilePoolKey);
    }

    /// <summary>Upgrade the tower to the next level. Returns true on success.</summary>
    public bool Upgrade()
    {
        if (!CanUpgrade) return false;
        UpgradeLevel++;
        currentRange    = Data.range * (1f + UpgradeLevel * 0.30f);     // +30% range per level
        currentCooldown = Data.attackCooldown * Mathf.Pow(0.80f, UpgradeLevel); // -20% cd per level
        rangeIndicator.transform.localScale = Vector3.one * currentRange * 2f;
        if (upgradeLabel != null) upgradeLabel.text = new string('|', UpgradeLevel);
        return true;
    }

    public void ShowRange(bool show) => rangeIndicator.enabled = show;

    void OnMouseEnter() => ShowRange(true);
    void OnMouseExit()  => ShowRange(false);
}
