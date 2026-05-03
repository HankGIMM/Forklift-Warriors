using UnityEngine;

public class WaypointSpawner : MonoBehaviour
{
    public GameObject[] waypoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyWaypoints();
        else
            DisableWaypoints();
    }

    private void ApplyWaypoints()
    {
        if (waypoints != null)
        {
            foreach (var waypoint in waypoints)
            {
                if (waypoint != null)
                    waypoint.SetActive(true);
            }
        }
    }

    private void DisableWaypoints()
    {
        if (waypoints != null)
        {
            foreach (var waypoint in waypoints)
            {
                if (waypoint != null)
                    waypoint.SetActive(false);
            }
        }
    }

}
