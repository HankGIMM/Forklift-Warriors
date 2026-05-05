using UnityEngine;

public class workerToggle : MonoBehaviour
{
    public GameObject[] workers;
    public void OnToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyWorkers();
        else
            DisableWorkers();
    }

    private void ApplyWorkers()
    {
        if (workers != null)
        {
            foreach (var worker in workers)
            {
                if (worker != null)
                    worker.SetActive(true);
            }
        }
    }
    private void DisableWorkers()
    {
        if (workers != null)
        {
            foreach (var worker in workers)
            {
                if (worker != null)
                    worker.SetActive(false);
            }
        }
    }
}
