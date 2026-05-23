using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "TowerDefense/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public int cost;
    public float attackCooldown;   // seconds between attacks
    public float range;
    public float damage;
    public bool isAoE;
    public float aoeRadius;
    public bool isSlowing;
    public float slowFactor;       // 0-1 multiplier on enemy speed
    public float slowDuration;
    public float projectileSpeed = 8f;
    public Color towerColor = Color.cyan;
    public string description;
}
