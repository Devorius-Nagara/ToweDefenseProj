using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<string, Queue<GameObject>> pools = new();
    private readonly Dictionary<string, GameObject> prototypes = new();

    void Awake() => Instance = this;

    public void CreatePool(string key, GameObject prototype, int initialSize)
    {
        prototypes[key] = prototype;
        var queue = new Queue<GameObject>();
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(prototype, transform);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }
        pools[key] = queue;
    }

    public GameObject Get(string key)
    {
        if (pools.TryGetValue(key, out var queue) && queue.Count > 0)
        {
            var obj = queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        if (prototypes.TryGetValue(key, out var proto))
        {
            var obj = Instantiate(proto, transform);
            obj.SetActive(true);
            return obj;
        }
        Debug.LogError($"Pool key not found: {key}");
        return null;
    }

    public void Return(string key, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        if (!pools.ContainsKey(key)) pools[key] = new Queue<GameObject>();
        pools[key].Enqueue(obj);
    }
}
