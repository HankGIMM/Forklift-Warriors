using UnityEngine;

public class PickUp : MonoBehaviour
{
    public string tagName = "boxpallet";
    public GameObject snapPoint;
    public Vector3 unsnapAngle;
    public float angleThreshold = 5f;

    private Rigidbody boxPallet;
    private bool isSnapped = false;

    void Update()
    {
        if (isSnapped && boxPallet != null)
        {
            boxPallet.transform.position = snapPoint.transform.position;
            boxPallet.transform.rotation = snapPoint.transform.rotation;

            float currentYaw = snapPoint.transform.parent.localEulerAngles.y;
            float targetYaw = (unsnapAngle.y + 360f) % 360f;
            float yawDiff = Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw));

            Debug.Log($"Spokes Current: {currentYaw} Target: {targetYaw} Diff: {yawDiff}");

            if (yawDiff <= angleThreshold)
            {
                Unsnap();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Walk up to find the tagged parent since CheckOne/CheckTwo are children
        Transform root = other.transform;
        while (root.parent != null && !root.CompareTag(tagName))
        {
            root = root.parent;
        }

        if (!root.CompareTag(tagName) || isSnapped) return;

        boxPallet = root.GetComponent<Rigidbody>();

        if (boxPallet != null)
        {
            boxPallet.isKinematic = true;
            root.SetParent(snapPoint.transform);
            isSnapped = true;
            Debug.Log("Snapped: " + root.name);
        }
    }

    private void Unsnap()
    {
        isSnapped = false;
        boxPallet.transform.SetParent(null);
        boxPallet.isKinematic = false;
        boxPallet = null;
        Debug.Log("Unsnapped - dropped!");
    }
}