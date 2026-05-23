using UnityEngine;

public class Projectile : MonoBehaviour
{
    private EnemyController target;
    private float speed;
    private float damage;
    private bool isAoE;
    private float aoeRadius;
    private bool isSlow;
    private float slowFactor;
    private float slowDuration;
    private string poolKey;

    /// <summary>If non-zero the projectile sprite spins at this speed (deg/sec). Set on the prototype.</summary>
    public float rotationSpeed = 0f;
    /// <summary>If true the sprite always faces the current target (good for arrows).</summary>
    public bool faceTarget = false;

    public void Launch(EnemyController enemy, TowerData towerData, string key)
    {
        target     = enemy;
        speed      = towerData.projectileSpeed;
        damage     = towerData.damage;
        isAoE      = towerData.isAoE;
        aoeRadius  = towerData.aoeRadius;
        isSlow     = towerData.isSlowing;
        slowFactor = towerData.slowFactor;
        slowDuration = towerData.slowDuration;
        poolKey    = key;
    }

    void Update()
    {
        if (target == null || !target.IsAlive)
        {
            ReturnToPool();
            return;
        }

        // Face target (arrows) or spin (fire/ice)
        if (faceTarget && target != null)
        {
            Vector3 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else if (rotationSpeed != 0f)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(
            transform.position, target.transform.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.transform.position) < 0.15f)
            Hit();
    }

    private void Hit()
    {
        if (isAoE)
        {
            // Damage all active enemies within aoe radius
            foreach (var enemy in EnemySpawner.Instance.ActiveEnemies.ToArray())
            {
                if (enemy == null || !enemy.IsAlive) continue;
                if (Vector3.Distance(transform.position, enemy.transform.position) <= aoeRadius)
                {
                    ApplyEffect(enemy);
                }
            }
        }
        else
        {
            if (target != null && target.IsAlive)
                ApplyEffect(target);
        }

        ReturnToPool();
    }

    private void ApplyEffect(EnemyController enemy)
    {
        enemy.TakeDamage(damage);
        if (isSlow && enemy.IsAlive)
            enemy.ApplySlow(slowFactor, slowDuration);
    }

    private void ReturnToPool() => PoolManager.Instance.Return(poolKey, gameObject);
}
