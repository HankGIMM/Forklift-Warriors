using UnityEngine;
using System.Collections;

public class PickUp : MonoBehaviour
{
    public string tagName = "boxpallet";
    public GameObject snapPoint;
    public Vector3 unsnapAngle;
    public float angleThreshold = 5f;
    public float unsnapCooldown = 2f;

    private Rigidbody boxPallet;
    private bool isSnapped = false;
    private float unsnapTime = -1f;
    private bool hasMovedAwayFromUnsnapAngle = false;

    void Update()
    {
        if (isSnapped && boxPallet != null)
        {
            boxPallet.transform.position = snapPoint.transform.position;
            boxPallet.transform.rotation = snapPoint.transform.rotation;

            float currentYaw = snapPoint.transform.parent.localEulerAngles.y;
            float targetYaw = (unsnapAngle.y + 360f) % 360f;
            float yawDiff = Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw));

            Debug.Log($"Current: {currentYaw} Target: {targetYaw} Diff: {yawDiff} MovedAway: {hasMovedAwayFromUnsnapAngle}");

            if (!hasMovedAwayFromUnsnapAngle)
            {
                if (yawDiff > angleThreshold * 3f)
                {
                    hasMovedAwayFromUnsnapAngle = true;
                }
                return;
            }

            if (yawDiff <= angleThreshold)
            {
                Unsnap();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < unsnapTime + unsnapCooldown) return;

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
            hasMovedAwayFromUnsnapAngle = false;
            Debug.Log("Snapped: " + root.name);
        }
    }

    private void Unsnap()
    {
        isSnapped = false;
        unsnapTime = Time.time;

        // Get all colliders on the pallet and spokes, ignore between them temporarily
        Collider[] palletColliders = boxPallet.GetComponentsInChildren<Collider>();
        Collider[] spokesColliders = snapPoint.transform.parent.GetComponentsInChildren<Collider>();

        foreach (Collider palletCol in palletColliders)
            foreach (Collider spokesCol in spokesColliders)
                Physics.IgnoreCollision(palletCol, spokesCol, true);

        StartCoroutine(ReEnableCollision(palletColliders, spokesColliders, unsnapCooldown));

        boxPallet.transform.SetParent(null);
        boxPallet.isKinematic = false;
        boxPallet = null;
        Debug.Log("Unsnapped - dropped!");
    }

    private IEnumerator ReEnableCollision(Collider[] palletColliders, Collider[] spokesColliders, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (Collider palletCol in palletColliders)
            foreach (Collider spokesCol in spokesColliders)
                if (palletCol != null && spokesCol != null)
                    Physics.IgnoreCollision(palletCol, spokesCol, false);

        Debug.Log("Collision re-enabled");
    }
}