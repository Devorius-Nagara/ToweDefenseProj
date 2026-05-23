using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    public EnemyData Data    { get; private set; }
    public string    PoolKey { get; private set; }
    public bool      IsAlive => gameObject.activeSelf;

    private float currentHP, currentSpeed, slowTimer;
    private int   waypointIndex;

    private SpriteRenderer sr;      // main body sprite
    private SpriteRenderer hitSR;   // overlay for hit flash
    private Transform hpBarFg;

    // Walk bob state
    private float bobTime;
    private Vector3 baseScale;
    private bool isDying;

    public float PathProgress
        => WaypointPath.Instance.GetProgress(waypointIndex, transform.position);

    // ── Mapping: enemy name → sprite resource path ───────────────────
    private static string SpritePathFor(string enemyName) => enemyName switch
    {
        "Goblin" => "Sprites/green-goblin",
        "Orc"    => "Sprites/blue-goblin",
        "Ghost"  => "Sprites/red-goblin",
        _        => null
    };

    public void Init(EnemyData data, string poolKey)
    {
        Data         = data;
        PoolKey      = poolKey;
        currentHP    = data.maxHealth;
        currentSpeed = data.moveSpeed;
        slowTimer    = 0f;
        waypointIndex = 1;
        isDying      = false;
        bobTime      = Random.value * Mathf.PI * 2f; // random phase so all don't bob in sync

        sr = GetComponent<SpriteRenderer>();

        // Load custom goblin sprite
        var spritePath = SpritePathFor(data.enemyName);
        if (spritePath != null)
        {
            var customSprite = SpriteLoader.Load(spritePath);
            if (customSprite != null)
            {
                sr.sprite = customSprite;
                sr.color  = Color.white; // don't tint — the sprite has its own colours
            }
            else
            {
                // Fallback: coloured circle
                sr.sprite = SpriteFactory.CreateCircle(data.enemyColor,
                    Color.Lerp(data.enemyColor, Color.black, 0.5f), 64);
                sr.color = data.enemyColor;
            }
        }
        else
        {
            sr.color = data.enemyColor;
        }
        sr.sortingOrder = 5;

        // Hit-flash overlay renderer (child GO, initially hidden)
        if (hitSR == null)
        {
            var hitGo = new GameObject("HitFlash");
            hitGo.transform.SetParent(transform, false);
            hitGo.transform.localPosition = Vector3.zero;
            hitGo.transform.localScale    = Vector3.one;
            hitSR = hitGo.AddComponent<SpriteRenderer>();
            hitSR.sortingOrder = 6;
            hitSR.enabled      = false;
        }
        // Always load hit sprite
        var hitSprite = SpriteLoader.Load("Sprites/goblin-hit");
        hitSR.sprite = hitSprite != null
            ? hitSprite
            : SpriteFactory.CreateCircle(new Color(1f, 1f, 0f, 0.8f), Color.white, 64);

        // Size / base scale
        float scale = data.enemyName switch
        {
            "Orc"   => 0.70f,
            "Ghost" => 0.78f,
            _       => 0.58f  // Goblin
        };
        transform.localScale = Vector3.one * scale;
        baseScale            = transform.localScale;

        hpBarFg = transform.Find("HPBar_FG");
        RefreshHPBar();

        // Reset transform state from previous pool use
        transform.rotation = Quaternion.identity;
        var col = sr.color; col.a = 1f; sr.color = col;
    }

    // ─────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!IsAlive || isDying) return;
        var state = GameManager.Instance.State;
        if (state == GameState.Defeat || state == GameState.Victory) return;
        if (state != GameState.Battle) return;

        // Slow timer
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                currentSpeed  = Data.moveSpeed;
                var c = sr.color; c.b = 1f; sr.color = c; // remove blue tint
            }
        }

        // Walk bob: gentle up-down sine + slight x-squash-stretch
        bobTime += Time.deltaTime * 6f;
        float bobY  = Mathf.Sin(bobTime) * 0.035f;
        float bobSx = 1f + Mathf.Cos(bobTime * 2f) * 0.04f;
        transform.localScale = new Vector3(baseScale.x * bobSx, baseScale.y * (1f - bobY * 0.4f), 1f);

        // Flip sprite to face movement direction
        var wp = WaypointPath.Instance;
        if (waypointIndex < wp.Count)
        {
            float dx = wp.GetWaypoint(waypointIndex).x - transform.position.x;
            if (Mathf.Abs(dx) > 0.05f)
                sr.flipX = dx < 0f;
        }

        MoveAlongPath();
    }

    // ─────────────────────────────────────────────────────────────────
    private void MoveAlongPath()
    {
        var path = WaypointPath.Instance;
        if (waypointIndex >= path.Count)
        {
            GameManager.Instance.DamageBase(Data.baseDamage);
            Finish();
            return;
        }
        Vector3 target = path.GetWaypoint(waypointIndex);
        transform.position = Vector3.MoveTowards(
            transform.position, target, currentSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.05f)
        { transform.position = target; waypointIndex++; }
    }

    // ─────────────────────────────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        if (!IsAlive || isDying) return;
        currentHP -= amount;
        RefreshHPBar();
        AudioManager.Instance?.PlayEnemyHurt();

        // Show hit-flash for 0.1 s
        StopCoroutine("HitFlashCo");
        StartCoroutine("HitFlashCo");

        if (currentHP <= 0f) StartCoroutine(DieCo());
    }

    private IEnumerator HitFlashCo()
    {
        hitSR.enabled = true;
        yield return new WaitForSeconds(0.1f);
        hitSR.enabled = false;
    }

    // ─────────────────────────────────────────────────────────────────
    public void ApplySlow(float factor, float duration)
    {
        if (!IsAlive || Data.immuneToSlow) return;
        currentSpeed = Data.moveSpeed * factor;
        slowTimer    = duration;
        var c = sr.color; c.b = Mathf.Min(c.b + 0.35f, 1f); sr.color = c;
    }

    // ─────────────────────────────────────────────────────────────────
    private IEnumerator DieCo()
    {
        isDying = true;
        EconomyManager.Instance.AddGold(Data.goldReward);
        StatisticsManager.Instance?.OnEnemyKilled();
        AudioManager.Instance?.PlayDeath();

        // Death animation: spin + shrink + fade over 0.25 s
        float t = 0f, dur = 0.25f;
        Vector3 startScale = transform.localScale;
        while (t < dur)
        {
            t += Time.deltaTime;
            float ratio = t / dur;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, ratio);
            transform.Rotate(0f, 0f, 720f * Time.deltaTime);
            var c = sr.color; c.a = 1f - ratio; sr.color = c;
            yield return null;
        }
        Finish();
    }

    // ─────────────────────────────────────────────────────────────────
    private void Finish()
    {
        isDying = false;
        if (hitSR != null) hitSR.enabled = false;
        EnemySpawner.Instance.ActiveEnemies.Remove(this);
        EnemySpawner.Instance.OnEnemyDefeated();
        PoolManager.Instance.Return(PoolKey, gameObject);
    }

    // ─────────────────────────────────────────────────────────────────
    private void RefreshHPBar()
    {
        if (hpBarFg == null) return;
        float ratio = Mathf.Clamp01(currentHP / Data.maxHealth);
        var ls = hpBarFg.localScale; ls.x = ratio * 0.9f; hpBarFg.localScale = ls;
        var lp = hpBarFg.localPosition; lp.x = -0.45f * (1f - ratio); hpBarFg.localPosition = lp;
    }

    public void AttachHealthBar(Transform bg, Transform fg) { hpBarFg = fg; }
}
