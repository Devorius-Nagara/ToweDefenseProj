using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple AI that fills the attack budget with enemies.
/// Strategy: escalates complexity with each round — early rounds favor cheap enemies,
/// later rounds mix in expensive ones.
/// </summary>
public class AIWaveBuilder : MonoBehaviour
{
    public static AIWaveBuilder Instance { get; private set; }

    private EnemyData[] enemyTypes;
    private const int MaxEnemiesPerWave = 50;

    void Awake() => Instance = this;

    public void SetEnemyTypes(EnemyData[] types) => enemyTypes = types;

    public Queue<EnemyData> BuildWave(int budget, int round)
    {
        var wave = new Queue<EnemyData>();
        int remaining = budget;
        int count = 0;

        // Sort enemies by cost ascending
        var sorted = new List<EnemyData>(enemyTypes);
        sorted.Sort((a, b) => a.waveCost.CompareTo(b.waveCost));

        // Unlock stronger enemies gradually by round
        int maxTierIndex = Mathf.Min(round - 1, sorted.Count - 1);

        var candidates = sorted.GetRange(0, maxTierIndex + 1);

        // Fill wave greedily with some randomness
        while (remaining > 0 && count < MaxEnemiesPerWave)
        {
            // Pick a random enemy from affordable candidates
            var affordable = candidates.FindAll(e => e.waveCost <= remaining);
            if (affordable.Count == 0) break;

            // Bias toward expensive ones in later rounds
            EnemyData chosen;
            float roll = Random.value;
            if (roll < 0.3f + round * 0.05f && affordable.Count > 1)
                chosen = affordable[affordable.Count - 1]; // most expensive affordable
            else
                chosen = affordable[Random.Range(0, affordable.Count)];

            wave.Enqueue(chosen);
            remaining -= chosen.waveCost;
            count++;
        }

        Debug.Log($"[AI] Round {round} — budget {budget}, spawning {wave.Count} enemies");
        return wave;
    }
}
