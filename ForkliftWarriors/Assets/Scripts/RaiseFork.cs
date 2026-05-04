using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

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
    [SerializeField] private float forkTiltSpeed  = 45f;

    [Header("Limits")]
    [SerializeField] private float minY, maxY;
    [SerializeField] private float minZ, maxZ;
    [SerializeField] private float minTilt, maxTilt;

    [Header("Audio")]
    [SerializeField] private AudioSource accessibleSound_Raise_Lower;
    [SerializeField] private AudioSource accessibleSound_Left_Right;
    [SerializeField] private AudioSource accessibleSound_Tilt;

    [Header("Subtitles")]
    [SerializeField] private TextMeshProUGUI RaiseLower_subtitleText;
    [SerializeField] private TextMeshProUGUI LeftRight_subtitleText;
    [SerializeField] private TextMeshProUGUI Tilt_subtitleText;

    private Vector3    restPosition;
    private Quaternion restRotation;
    private float currentTilt = 0f;
    private const float deadzone = 0.1f;

    void Start()
    {
        restPosition = forkTransform.localPosition;
        restRotation = forkTransform.localRotation;

        // Existing accessible-sound hover listeners — unchanged.
        raiseLever.hoverEntered.AddListener(_ => accessibleSound_Raise_Lower.Play());
        raiseLever.hoverExited.AddListener(_ => accessibleSound_Raise_Lower.Stop());
        raiseLever.hoverEntered.AddListener(_ => RaiseLower_subtitleText.text = "Raising/Lowering Fork");
        raiseLever.hoverExited.AddListener(_ => RaiseLower_subtitleText.text = "");

        leftRightLever.hoverEntered.AddListener(_ => accessibleSound_Left_Right.Play());
        leftRightLever.hoverExited.AddListener(_ => accessibleSound_Left_Right.Stop());
        leftRightLever.hoverEntered.AddListener(_ => LeftRight_subtitleText.text = "Sliding Fork Left/Right");
        leftRightLever.hoverExited.AddListener(_ => LeftRight_subtitleText.text = "");

        tiltLever.hoverEntered.AddListener(_ => accessibleSound_Tilt.Play());
        tiltLever.hoverExited.AddListener(_ => accessibleSound_Tilt.Stop());
        tiltLever.hoverEntered.AddListener(_ => Tilt_subtitleText.text = "Tilting Fork Forward/Backward");
        tiltLever.hoverExited.AddListener(_ => Tilt_subtitleText.text = "");
    }

    void OnDestroy()
    {
        raiseLever.hoverEntered.RemoveAllListeners();
        raiseLever.hoverExited.RemoveAllListeners();
        leftRightLever.hoverEntered.RemoveAllListeners();
        leftRightLever.hoverExited.RemoveAllListeners();
        tiltLever.hoverEntered.RemoveAllListeners();
        tiltLever.hoverExited.RemoveAllListeners();
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

    float GetLeverInput(XRLever lever)
    {
        if (!lever.isSelected) return 0f;
        return ApplyDeadzone(lever.value);
    }

    void RaisingFork()
    {
        float input = GetLeverInput(raiseLever);

        // *** AUDIO: broadcast raise/lower input every frame (including zero
        //     so ForkliftAudioManager knows when to stop the looping clip).
        ForkliftEvents.RaiseForkRaising(input);

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

        // *** AUDIO: broadcast slide input every frame.
        ForkliftEvents.RaiseForkSliding(input);

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

        // *** AUDIO: broadcast tilt input every frame.
        ForkliftEvents.RaiseForkTilting(input);

        if (input != 0f)
        {
            currentTilt += input * forkTiltSpeed * Time.deltaTime;
            currentTilt = Mathf.Clamp(currentTilt, minTilt, maxTilt);
            forkTransform.localRotation = restRotation * Quaternion.Euler(currentTilt, 0f, 0f);
        }
    }
}