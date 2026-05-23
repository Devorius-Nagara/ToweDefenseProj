using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    public List<EnemyController> ActiveEnemies { get; } = new();
    public Queue<EnemyData> PendingWave { get; set; } = new();

    private float spawnInterval = 1.0f;
    private int defeatedCount;
    private int totalToSpawn;
    private bool spawnFinished;

    void Awake() => Instance = this;

    /// <summary>Starts spawning the queued wave with the given interval.</summary>
    public void SpawnWave(Queue<EnemyData> wave, float interval = 1.0f)
    {
        spawnInterval  = interval;
        defeatedCount  = 0;
        totalToSpawn   = wave.Count;
        spawnFinished  = false;
        StartCoroutine(SpawnRoutine(wave));
    }

    private IEnumerator SpawnRoutine(Queue<EnemyData> wave)
    {
        while (wave.Count > 0)
        {
            SpawnOne(wave.Dequeue());
            yield return new WaitForSeconds(spawnInterval);
        }
        spawnFinished = true;
        // Edge case: all enemies already handled before last spawn tick
        CheckWaveComplete();
    }

    private void SpawnOne(EnemyData data)
    {
        string key = "enemy_" + data.enemyName;
        GameObject go = PoolManager.Instance.Get(key);
        go.transform.position = WaypointPath.Instance.GetWaypoint(0);
        go.transform.localScale = Vector3.one;
        go.transform.rotation   = Quaternion.identity;

        var ctrl = go.GetComponent<EnemyController>();
        ctrl.Init(data, key);
        ActiveEnemies.Add(ctrl);
    }

    public void OnEnemyDefeated()
    {
        defeatedCount++;
        CheckWaveComplete();
    }

    private void CheckWaveComplete()
    {
        // Wave is complete when all enemies have been spawned AND all handled
        if (spawnFinished && defeatedCount >= totalToSpawn && ActiveEnemies.Count == 0)
            GameManager.Instance.OnWaveComplete();
    }

    /// <summary>Stops all spawning and returns active enemies to the pool.</summary>
    public void ClearAll()
    {
        StopAllCoroutines();

        foreach (var e in ActiveEnemies.ToArray())
            if (e != null && e.gameObject.activeSelf)
                PoolManager.Instance.Return(e.PoolKey, e.gameObject);

        ActiveEnemies.Clear();
        defeatedCount = 0;
        totalToSpawn  = 0;
        spawnFinished = false;
        PendingWave   = new Queue<EnemyData>();
    }
}
