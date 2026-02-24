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

    void FixedUpdate()
    {
        // 1. Get Joystick Input
        Vector2 leftHandInput = m_LeftHandMoveInput.ReadValue();
        Vector2 rightHandInput = m_RightHandMoveInput.ReadValue();

        float verticalInput = leftHandInput.y;   // Left joystick Forward/Backward
        float horizontalInput = rightHandInput.x; // Right joystick Left/Right

        print(horizontalInput);

        // 2. Handle Steering
        if (Mathf.Abs(horizontalInput) > 0.05f) // Small deadzone
        {
            // Calculate rotation amount based on input and turn speed
            float turnAmount = horizontalInput * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnOffset = Quaternion.Euler(0, turnAmount, 0);
            
            // MoveRotation is the most stable way to turn a rigid body
            rb.MoveRotation(rb.rotation * turnOffset);
        }

        // 3. Prevent Lateral "Ice Skating" Drift
        // Convert world velocity to local velocity to isolate forward/sideways/upward speed
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        
        // Rapidly kill sideways velocity (X axis) to simulate tire grip
        localVelocity.x = Mathf.Lerp(localVelocity.x, 0, Time.fixedDeltaTime * 10f);
        rb.linearVelocity = transform.TransformDirection(localVelocity);

        // Update local speed reference after killing drift
        float currentForwardSpeed = localVelocity.z;

        // 4. Handle Acceleration & Braking
        if (Mathf.Abs(verticalInput) > 0.05f) // If pushing joystick
        {
            float targetAcceleration = (verticalInput > 0) ? maxAcceleration : maxReverseAccel;
            
            // Only apply forward force if we haven't hit max speed yet
            if (Mathf.Abs(currentForwardSpeed) < maxSpeed)
            {
                Vector3 accelerationVector = transform.forward * (verticalInput * targetAcceleration);
                rb.AddForce(accelerationVector, ForceMode.Acceleration);
            }
        }
        else // Braking (joystick released)
        {
            // If we are still moving forward/backward
            if (Mathf.Abs(currentForwardSpeed) > 0.1f)
            {
                // Apply a strong counter-force (maxDeceleration) in the opposite direction
                float brakeDirection = Mathf.Sign(currentForwardSpeed) * -1f;
                Vector3 brakeVector = transform.forward * (maxDeceleration * brakeDirection);
                rb.AddForce(brakeVector, ForceMode.Acceleration);
            }
            else
            {
                // Full stop. If we get close to 0, snap z velocity to 0 to prevent micro-drifting
                localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
                localVelocity.z = 0;
                rb.linearVelocity = transform.TransformDirection(localVelocity);
            }
        }
    }
}