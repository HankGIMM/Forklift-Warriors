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
    [SerializeField] float maxSpeed = 5.0f; // Added a top speed limit so it doesn't accelerate infinitely
    [SerializeField] float maxAcceleration = 5.0f; // m/s^2
    [SerializeField] float maxDeceleration = 10.0f; // m/s^2 (stronger braking)
    [SerializeField] float maxReverseAccel = 3.0f;
    
    [Header("Steering Settings")]
    [SerializeField] float turnSpeed = 90.0f; // Degrees per second

    [Header("References")]
    [SerializeField] Rigidbody rb;

    void Start()
{
    // Kill any velocity the Rigidbody may have accumulated before first input frame
    if (rb != null)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}

void FixedUpdate()
{
    // 1. Get Joystick Input
    Vector2 leftHandInput = m_LeftHandMoveInput.ReadValue();
    Vector2 rightHandInput = m_RightHandMoveInput.ReadValue();

    float verticalInput   = leftHandInput.y;
    float horizontalInput = rightHandInput.x;

    // 2. Handle Steering (unchanged — this part was fine)
    if (Mathf.Abs(horizontalInput) > 0.05f)
    {
        float turnAmount = horizontalInput * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turnAmount, 0));
    }

    // 3. Kill lateral drift (unchanged — this part was fine)
    Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
    localVelocity.x = Mathf.Lerp(localVelocity.x, 0, Time.fixedDeltaTime * 10f);

    float currentForwardSpeed = localVelocity.z;

    // 4. Acceleration & Braking via MoveTowards (no more AddForce oscillation)
    float newForwardSpeed;

    if (Mathf.Abs(verticalInput) > 0.05f)
    {
        // Determine target speed based on input direction
        float targetSpeed = verticalInput * maxSpeed; // handles both forward and reverse
        float accel = (verticalInput > 0) ? maxAcceleration : maxReverseAccel;
        newForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, targetSpeed, accel * Time.fixedDeltaTime);
    }
    else
    {
        // Braking: MoveTowards(current, 0) will NEVER overshoot past zero
        newForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, 0f, maxDeceleration * Time.fixedDeltaTime);
    }

    localVelocity.z = newForwardSpeed;
    rb.linearVelocity = transform.TransformDirection(localVelocity);
}
}