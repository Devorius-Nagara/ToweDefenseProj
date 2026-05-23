using UnityEngine;
using System.Collections.Generic;

public class WaypointPath : MonoBehaviour
{
    public static WaypointPath Instance { get; private set; }

    private readonly List<Vector3> waypoints = new List<Vector3>();

    void Awake() => Instance = this;

    public void SetWaypoints(List<Vector3> wps) => waypoints.AddRange(wps);

    public int Count => waypoints.Count;
    public Vector3 GetWaypoint(int index) => waypoints[index];

    /// <summary>Returns 0-1 progress of a position along the path.</summary>
    public float GetProgress(int waypointIndex, Vector3 position)
    {
        if (waypoints.Count == 0) return 0f;
        float total = 0f;
        for (int i = 1; i < waypoints.Count; i++)
            total += Vector3.Distance(waypoints[i - 1], waypoints[i]);

        float done = 0f;
        for (int i = 1; i < waypointIndex && i < waypoints.Count; i++)
            done += Vector3.Distance(waypoints[i - 1], waypoints[i]);

        if (waypointIndex > 0 && waypointIndex <= waypoints.Count - 1)
            done += Vector3.Distance(waypoints[waypointIndex - 1], position);

        return total > 0 ? done / total : 0f;
    }
}
