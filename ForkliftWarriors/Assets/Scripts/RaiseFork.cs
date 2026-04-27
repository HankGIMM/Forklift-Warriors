using UnityEngine;

public class RaiseFork : MonoBehaviour
{
    [Header("Levers")]
    [SerializeField] XRLever raiseLever;
    [SerializeField] XRLever leftRightLever;
    [SerializeField] XRLever tiltLever;
    [Header("Fork Reference")]
    [SerializeField] Transform forkTransform;
    [Header("Speeds")]
    [SerializeField] private float forkRaiseSpeed = 1f;
    [SerializeField] private float forkSlideSpeed = 1f;
    [SerializeField] private float forkTiltSpeed = 45f;
    [Header("Limits")]
    [SerializeField] private float minY, maxY;
    [SerializeField] private float minZ, maxZ;
    [SerializeField] private float minTilt, maxTilt;
    private Vector3 restPosition;
    private Quaternion restRotation;
    private float currentTilt = 0f;
    private const float deadzone = 0.1f;

    void Start()
    {
        restPosition = forkTransform.localPosition;
        restRotation = forkTransform.localRotation;
    }

    void Update()
    {
        RaisingFork();
        LeftRightFork();
        TiltFork();
    }

    float ApplyDeadzone(float input)
    {
        if (Mathf.Abs(input) < deadzone) return 0f;
        return Mathf.Sign(input) * (Mathf.Abs(input) - deadzone) / (1f - deadzone);
    }

    // Returns the lever's input value only if it is currently being held,
    // otherwise returns 0 so the fork locks in place immediately on release.
    float GetLeverInput(XRLever lever)
    {
        if (!lever.isSelected) return 0f;
        return ApplyDeadzone(lever.value);
    }

    void RaisingFork()
    {
        float input = GetLeverInput(raiseLever);
        if (input != 0f)
        {
            Vector3 pos = forkTransform.localPosition;
            pos.y += input * forkRaiseSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, restPosition.y + minY, restPosition.y + maxY);
            forkTransform.localPosition = pos;
        }
    }

    void LeftRightFork()
    {
        float input = GetLeverInput(leftRightLever);
        if (input != 0f)
        {
            Vector3 pos = forkTransform.localPosition;
            pos.x += input * forkSlideSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, restPosition.x + minZ, restPosition.x + maxZ);
            forkTransform.localPosition = pos;
        }
    }

    void TiltFork()
    {
        float input = GetLeverInput(tiltLever);
        if (input != 0f)
        {
            currentTilt += input * forkTiltSpeed * Time.deltaTime;
            currentTilt = Mathf.Clamp(currentTilt, minTilt, maxTilt);
            forkTransform.localRotation = restRotation * Quaternion.Euler(currentTilt, 0f, 0f);
        }
    }
}