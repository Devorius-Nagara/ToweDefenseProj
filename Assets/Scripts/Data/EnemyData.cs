using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "TowerDefense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float maxHealth;
    public float moveSpeed;
    public int goldReward;
    public int waveCost;       // cost in attacker budget
    public int baseDamage;     // damage to base HP on arrival
    public bool immuneToSlow;  // Ghost ignores Freezer
    public Color enemyColor = Color.red;
}
