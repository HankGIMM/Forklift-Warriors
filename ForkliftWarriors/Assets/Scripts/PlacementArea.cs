using UnityEngine;

public class PlacementArea : MonoBehaviour
{
    private Rigidbody boxPallet;
    private bool isSnapped;
    public GameObject snapPoint;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("boxpallet") && !isSnapped)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
                Snap(other.transform, rb);
        }
    }

    private void Snap(Transform root, Rigidbody rb)
    {
        boxPallet = rb;
        boxPallet.isKinematic = true;
        root.SetParent(snapPoint.transform);
        root.position = snapPoint.transform.position;
        root.rotation = snapPoint.transform.rotation;
        isSnapped = true;

       
        foreach (Collider col in root.GetComponentsInChildren<Collider>())
        {
            col.enabled = false; 
        }
        Debug.Log("Snapped permanently: " + root.name);
    }
}