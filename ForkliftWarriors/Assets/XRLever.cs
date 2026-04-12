using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// XRBaseInteractable handles all the VR grab detection without needing
// a Rigidbody or HingeJoint — much cleaner than physics-based levers
public class XRLever : XRBaseInteractable
{
    [SerializeField]
    [Tooltip("The cylinder/handle mesh that visually rotates when the lever is grabbed")]
    Transform m_Handle = null;

    [SerializeField]
    [Tooltip("How far the lever can be pushed forward (positive = forward)")]
    [Range(-90.0f, 90.0f)]
    float m_MaxAngle = 60.0f;

    [SerializeField]
    [Tooltip("How far the lever can be pulled back (negative = back)")]
    [Range(-90.0f, 90.0f)]
    float m_MinAngle = -60.0f;

    [SerializeField]
    [Tooltip("If true, lever springs back to centre when released — good for raise/lower. " +
             "If false, lever stays where released — good for side shift and tilt")]
    bool m_ReturnToCenter = false;

    [SerializeField]
    [Tooltip("How fast the lever returns to centre when released (only used if ReturnToCenter is true)")]
    float m_ReturnSpeed = 5f;

    // Output value normalized between -1 and 1
    // -1 = fully pulled back (min angle)
    //  0 = centre/neutral
    //  1 = fully pushed forward (max angle)
    // RaiseFork reads this to know how fast and which direction to move the fork
    public float value { get; private set; }

    // Raw angle in degrees — useful for debugging
    public float angle { get; private set; }

    // Stores reference to whichever hand is currently grabbing this lever
    IXRSelectInteractor m_Interactor;
    bool m_IsGrabbed = false;

    // Public accessors so other scripts can read/write these if needed
    public Transform handle { get => m_Handle; set => m_Handle = value; }
    public float maxAngle { get => m_MaxAngle; set => m_MaxAngle = value; }
    public float minAngle { get => m_MinAngle; set => m_MinAngle = value; }

    protected override void OnEnable()
    {
        base.OnEnable();
        // subscribe to grab events when this object becomes active
        selectEntered.AddListener(StartGrab);
        selectExited.AddListener(EndGrab);
    }

    protected override void OnDisable()
    {
        // always unsubscribe when disabled to prevent memory leaks
        selectEntered.RemoveListener(StartGrab);
        selectExited.RemoveListener(EndGrab);
        base.OnDisable();
    }

    // called the moment the player grabs the lever
    void StartGrab(SelectEnterEventArgs args)
    {
        m_Interactor = args.interactorObject; // store which hand grabbed it
        m_IsGrabbed = true;
    }

    // called the moment the player releases the lever
    void EndGrab(SelectExitEventArgs args)
    {
        m_Interactor = null;
        m_IsGrabbed = false;
        // if ReturnToCenter is false, lever stays exactly where released
        // RaiseFork will keep reading the last value, holding the fork in place
    }

    // ProcessInteractable runs every frame on XR objects, like Update() but for interactables
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (m_IsGrabbed && m_Interactor != null)
                UpdateValue();          // hand is holding lever — follow hand position
            else if (m_ReturnToCenter)
                ReturnToCenter();       // released and ReturnToCenter is on — spring back
            // if not grabbed and ReturnToCenter is off — do nothing, lever stays put
        }
    }

    // calculates which direction the hand is relative to the lever handle
    // strips out the X axis so the lever only rotates on one axis (forward/back)
    Vector3 GetLookDirection()
    {
        Vector3 direction = m_Interactor.GetAttachTransform(this).position - m_Handle.position;
        direction = transform.InverseTransformDirection(direction); // convert to lever local space
        direction.x = 0; // lock to forward/back axis only — prevents sideways wobble
        return direction.normalized;
    }

    // called every frame while grabbed — rotates the handle to follow the hand
    void UpdateValue()
    {
        var lookDirection = GetLookDirection();

        // convert the direction vector to an angle in degrees
        var lookAngle = Mathf.Atan2(lookDirection.z, lookDirection.y) * Mathf.Rad2Deg;

        // clamp so the lever can't go past its min/max limits
        lookAngle = Mathf.Clamp(lookAngle,
            Mathf.Min(m_MinAngle, m_MaxAngle),
            Mathf.Max(m_MinAngle, m_MaxAngle));

        SetHandleAngle(lookAngle);
    }

    // smoothly moves the lever back to 0 degrees (centre) when released
    void ReturnToCenter()
    {
        angle = Mathf.MoveTowards(angle, 0f, m_ReturnSpeed * Time.deltaTime);
        SetHandleAngle(angle);
    }

    // applies the angle to the handle mesh and calculates the normalized output value
    void SetHandleAngle(float newAngle)
    {
        angle = newAngle;

        // convert angle to -1 to 1 range so RaiseFork doesn't need to know about degrees
        // example: minAngle=-60, maxAngle=60, angle=30 → value = 0.5
        float range = m_MaxAngle - m_MinAngle;
        value = range != 0 ? ((angle - m_MinAngle) / range) * 2f - 1f : 0f;

        // physically rotate the handle mesh to match
        if (m_Handle != null)
            m_Handle.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);
    }

    // draws green/red lines in the scene view showing the lever's min/max range
    void OnDrawGizmosSelected()
    {
        var angleStartPoint = transform.position;
        if (m_Handle != null)
            angleStartPoint = m_Handle.position;

        const float k_AngleLength = 0.25f;

        var angleMaxPoint = angleStartPoint + transform.TransformDirection(
            Quaternion.Euler(m_MaxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
        var angleMinPoint = angleStartPoint + transform.TransformDirection(
            Quaternion.Euler(m_MinAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(angleStartPoint, angleMaxPoint);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(angleStartPoint, angleMinPoint);
    }

    // resets handle rotation in the editor when values are changed in Inspector
    void OnValidate()
    {
        if (m_Handle != null)
            m_Handle.localRotation = Quaternion.Euler(0f, 0.0f, 0.0f);
    }
}