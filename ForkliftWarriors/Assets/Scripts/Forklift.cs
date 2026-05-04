using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class Forklift : MonoBehaviour
{
    [Header("Input Action Readers")]
    [SerializeField]
    [Tooltip("Reads input data from the left hand controller. Input Action must be a Value action type (Vector 2).")]
    private XRInputValueReader<Vector2> m_LeftHandMoveInput = new XRInputValueReader<Vector2>("Left Hand Move");

    [SerializeField]
    [Tooltip("Reads input data from the right hand controller. Input Action must be a Value action type (Vector 2).")]
    private XRInputValueReader<Vector2> m_RightHandMoveInput = new XRInputValueReader<Vector2>("Right Hand Move");

    [Header("Movement Settings")]
    [SerializeField] float maxSpeed = 5.0f;
    [SerializeField] float maxAcceleration = 5.0f;
    [SerializeField] float maxDeceleration = 10.0f;
    [SerializeField] float maxReverseAccel = 3.0f;

    [Header("Steering Settings")]
    [SerializeField] float turnSpeed = 90.0f;

    [Header("References")]
    [SerializeField] Rigidbody rb;

    // *** AUDIO: track whether we were moving last frame so we can fire
    //     the brake event exactly once when the forklift comes to a stop.
    private bool _wasMoving = false;

    void Start()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        // 1. Get Joystick Input
        Vector2 leftHandInput  = m_LeftHandMoveInput.ReadValue();
        Vector2 rightHandInput = m_RightHandMoveInput.ReadValue();

        float verticalInput   = leftHandInput.y;
        float horizontalInput = rightHandInput.x;

        // 2. Handle Steering
        if (Mathf.Abs(horizontalInput) > 0.05f)
        {
            float turnAmount = horizontalInput * turnSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turnAmount, 0));
        }

        // 3. Kill lateral drift
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        localVelocity.x = Mathf.Lerp(localVelocity.x, 0, Time.fixedDeltaTime * 10f);

        float currentForwardSpeed = localVelocity.z;

        // 4. Acceleration & Braking
        float newForwardSpeed;

        if (Mathf.Abs(verticalInput) > 0.05f)
        {
            float targetSpeed = verticalInput * maxSpeed;
            float accel = (verticalInput > 0) ? maxAcceleration : maxReverseAccel;
            newForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, targetSpeed, accel * Time.fixedDeltaTime);
        }
        else
        {
            newForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, 0f, maxDeceleration * Time.fixedDeltaTime);
        }

        localVelocity.z = newForwardSpeed;
        rb.linearVelocity = transform.TransformDirection(localVelocity);

        // *** AUDIO: broadcast normalised speed every physics tick.
        //     ForkliftAudioManager uses this to pitch-shift the engine
        //     and swap between idle / drive clips.
        float normalisedSpeed = newForwardSpeed / maxSpeed;
        ForkliftEvents.RaiseForkliftSpeed(normalisedSpeed);

        // *** AUDIO: fire a one-shot brake event the moment we stop.
        bool isMovingNow = Mathf.Abs(newForwardSpeed) > 0.05f;
        if (_wasMoving && !isMovingNow)
            ForkliftEvents.RaiseForkliftBraked();
        _wasMoving = isMovingNow;
    }
}