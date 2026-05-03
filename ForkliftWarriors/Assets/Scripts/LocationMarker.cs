using System;
using UnityEngine;

public class LocationMarker : MonoBehaviour
{
    [Tooltip("Optional: specify a Tag that the XR hands GameObject uses. Leave empty to match by name keywords.")]
    public string targetTag = "Player";

    [Tooltip("If no tag is provided, any ancestor or collider name containing any of these keywords (case-insensitive) will match.")]
    public string[] targetNameKeywords = new[] { "Hand", "Hands", "XR Origin", "XROrigin", "HandCollider" };

    // Called when this collider is marked as a Trigger and another collider enters it
    private void OnTriggerEnter(Collider other)
    {
        if (IsXRHandsCollider(other))
            Destroy(gameObject);
    }

    // Called when a non-trigger collision occurs
    private void OnCollisionEnter(Collision collision)
    {
        if (IsXRHandsCollider(collision.collider))
            Destroy(gameObject);
    }

    private bool IsXRHandsCollider(Collider other)
    {
        if (other == null) return false;

        // Match by explicit tag first (if set)
        if (!string.IsNullOrEmpty(targetTag) && other.CompareTag(targetTag))
            return true;

        // Otherwise walk up the hierarchy and match by name keywords
        Transform t = other.transform;
        while (t != null)
        {
            string name = t.name ?? string.Empty;
            foreach (var kw in targetNameKeywords)
            {
                if (string.IsNullOrEmpty(kw)) continue;
                if (name.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            t = t.parent;
        }

        return false;
    }
}
