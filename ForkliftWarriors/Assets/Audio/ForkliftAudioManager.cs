// ForkliftAudioManager.cs
// Attach this to a dedicated "AudioManager" GameObject in your scene.
// Wire up AudioSources and AudioClips in the Inspector.
// This script knows nothing about game logic — it only listens to ForkliftEvents.

using UnityEngine;

public class ForkliftAudioManager : MonoBehaviour
{
    // ------------------------------------------------------------------
    // AudioSources
    // ------------------------------------------------------------------

    [Header("Audio Sources")]
    [Tooltip("Looping source for the engine — pitch-shifted by speed.")]
    [SerializeField] private AudioSource engineSource;

    [Tooltip("Looping source for fork movement sounds.")]
    [SerializeField] private AudioSource forkSource;

    [Tooltip("General one-shot source for short effects (snaps, UI, score, hits).")]
    [SerializeField] private AudioSource oneShotSource;

    // ------------------------------------------------------------------
    // Engine Clips
    // ------------------------------------------------------------------

    [Header("Engine Clips")]
    [SerializeField] private AudioClip engineIdle;
    [SerializeField] private AudioClip engineDrive;
    [SerializeField] private AudioClip brakeSound;

    // ------------------------------------------------------------------
    // Fork Clips
    // ------------------------------------------------------------------

    [Header("Fork Clips")]
    [SerializeField] private AudioClip forkRaiseClip;
    [SerializeField] private AudioClip forkSlideClip;
    [SerializeField] private AudioClip forkTiltClip;

    // ------------------------------------------------------------------
    // Lever Clips
    // ------------------------------------------------------------------

    [Header("Lever Clips")]
    [SerializeField] private AudioClip leverGrabClip;
    [SerializeField] private AudioClip leverReleaseClip;

    // ------------------------------------------------------------------
    // Pallet Clips
    // ------------------------------------------------------------------

    [Header("Pallet Clips")]
    [SerializeField] private AudioClip palletSnapClip;
    [SerializeField] private AudioClip palletDropClip;
    [SerializeField] private AudioClip palletPlacedClip;

    // ------------------------------------------------------------------
    // Score / UI Clips
    // ------------------------------------------------------------------

    [Header("Score / UI Clips")]
    [SerializeField] private AudioClip scoreUpClip;

    // ------------------------------------------------------------------
    // Worker Clips
    // ------------------------------------------------------------------

    [Header("Worker Clips")]
    [Tooltip("Add as many impact clips as you like — a random one plays on each hit. " +
             "Tip: slight pitch/tone variation between clips makes repeats less noticeable.")]
    [SerializeField] private AudioClip[] workerHitClips;

    [Tooltip("Groan or shuffle when a worker recovers from ragdoll.")]
    [SerializeField] private AudioClip workerRecoveredClip;

    // ------------------------------------------------------------------
    // Engine tuning
    // ------------------------------------------------------------------

    [Header("Engine Tuning")]
    [SerializeField] private float enginePitchIdle = 0.8f;
    [SerializeField] private float enginePitchMax  = 1.6f;

    private bool _wasMoving       = false;
    private bool _forkRaiseActive = false;
    private bool _forkSlideActive = false;
    private bool _forkTiltActive  = false;

    // Tracks the last clip index played so we never repeat the same hit
    // sound twice in a row (only matters when array has 2+ clips).
    private int _lastWorkerHitIndex = -1;

    // ------------------------------------------------------------------
    // Lifecycle
    // ------------------------------------------------------------------

    void Awake()
    {
        ValidateSources();
        StartEngineIdle();
    }

    void OnEnable()
    {
        ForkliftEvents.OnForkliftSpeedChanged += HandleForkliftSpeed;
        ForkliftEvents.OnForkliftBraked       += HandleBrake;

        ForkliftEvents.OnForkRaising   += HandleForkRaise;
        ForkliftEvents.OnForkSliding   += HandleForkSlide;
        ForkliftEvents.OnForkTilting   += HandleForkTilt;
        ForkliftEvents.OnLeverGrabbed  += HandleLeverGrabbed;
        ForkliftEvents.OnLeverReleased += HandleLeverReleased;

        ForkliftEvents.OnPalletSnapped += HandlePalletSnapped;
        ForkliftEvents.OnPalletDropped += HandlePalletDropped;
        ForkliftEvents.OnPalletPlaced  += HandlePalletPlaced;

        ForkliftEvents.OnScoreUpdated  += HandleScoreUpdated;

        ForkliftEvents.OnWorkerHit       += HandleWorkerHit;
        ForkliftEvents.OnWorkerRecovered += HandleWorkerRecovered;
    }

    void OnDisable()
    {
        ForkliftEvents.OnForkliftSpeedChanged -= HandleForkliftSpeed;
        ForkliftEvents.OnForkliftBraked       -= HandleBrake;

        ForkliftEvents.OnForkRaising   -= HandleForkRaise;
        ForkliftEvents.OnForkSliding   -= HandleForkSlide;
        ForkliftEvents.OnForkTilting   -= HandleForkTilt;
        ForkliftEvents.OnLeverGrabbed  -= HandleLeverGrabbed;
        ForkliftEvents.OnLeverReleased -= HandleLeverReleased;

        ForkliftEvents.OnPalletSnapped -= HandlePalletSnapped;
        ForkliftEvents.OnPalletDropped -= HandlePalletDropped;
        ForkliftEvents.OnPalletPlaced  -= HandlePalletPlaced;

        ForkliftEvents.OnScoreUpdated  -= HandleScoreUpdated;

        ForkliftEvents.OnWorkerHit       -= HandleWorkerHit;
        ForkliftEvents.OnWorkerRecovered -= HandleWorkerRecovered;
    }

    // ------------------------------------------------------------------
    // Engine handlers
    // ------------------------------------------------------------------

    private void HandleForkliftSpeed(float normalisedSpeed)
    {
        bool isMoving = Mathf.Abs(normalisedSpeed) > 0.05f;

        engineSource.pitch = Mathf.Lerp(enginePitchIdle, enginePitchMax, Mathf.Abs(normalisedSpeed));

        if (isMoving && !_wasMoving)
        {
            SwapEngineClip(engineDrive);
            _wasMoving = true;
        }
        else if (!isMoving && _wasMoving)
        {
            SwapEngineClip(engineIdle);
            _wasMoving = false;
        }
    }

    private void HandleBrake()
    {
        if (brakeSound != null)
            oneShotSource.PlayOneShot(brakeSound);
    }

    // ------------------------------------------------------------------
    // Fork handlers
    // ------------------------------------------------------------------

    private void HandleForkRaise(float input) => HandleForkLoop(ref _forkRaiseActive, forkRaiseClip, input);
    private void HandleForkSlide(float input) => HandleForkLoop(ref _forkSlideActive, forkSlideClip, input);
    private void HandleForkTilt(float input)  => HandleForkLoop(ref _forkTiltActive,  forkTiltClip,  input);

    private void HandleForkLoop(ref bool activeFlag, AudioClip clip, float input)
    {
        bool hasInput = Mathf.Abs(input) > 0.05f;

        if (hasInput && !activeFlag)
        {
            if (clip != null)
            {
                forkSource.clip  = clip;
                forkSource.loop  = true;
                forkSource.pitch = Mathf.Lerp(0.9f, 1.2f, Mathf.Abs(input));
                forkSource.Play();
            }
            activeFlag = true;
        }
        else if (!hasInput && activeFlag)
        {
            forkSource.Stop();
            activeFlag = false;
        }
        else if (hasInput)
        {
            forkSource.pitch = Mathf.Lerp(0.9f, 1.2f, Mathf.Abs(input));
        }
    }

    private void HandleLeverGrabbed()  { if (leverGrabClip    != null) oneShotSource.PlayOneShot(leverGrabClip);    }
    private void HandleLeverReleased() { if (leverReleaseClip != null) oneShotSource.PlayOneShot(leverReleaseClip); }

    // ------------------------------------------------------------------
    // Pallet handlers
    // ------------------------------------------------------------------

    private void HandlePalletSnapped() { if (palletSnapClip   != null) oneShotSource.PlayOneShot(palletSnapClip);   }
    private void HandlePalletDropped() { if (palletDropClip   != null) oneShotSource.PlayOneShot(palletDropClip);   }
    private void HandlePalletPlaced()  { if (palletPlacedClip != null) oneShotSource.PlayOneShot(palletPlacedClip); }

    // ------------------------------------------------------------------
    // Score handler
    // ------------------------------------------------------------------

    private void HandleScoreUpdated(int newScore)
    {
        if (scoreUpClip != null)
            oneShotSource.PlayOneShot(scoreUpClip);
    }

    // ------------------------------------------------------------------
    // Worker handlers
    // ------------------------------------------------------------------

    /// <summary>
    /// Picks a random clip from workerHitClips each time the forklift hits a worker.
    /// Avoids repeating the same clip twice in a row when there are multiple options.
    /// </summary>
    private void HandleWorkerHit()
    {
        if (workerHitClips == null || workerHitClips.Length == 0) return;

        int index;

        if (workerHitClips.Length == 1)
        {
            // Only one clip — just play it.
            index = 0;
        }
        else
        {
            // Keep re-rolling until we get a different index than last time.
            do { index = Random.Range(0, workerHitClips.Length); }
            while (index == _lastWorkerHitIndex);
        }

        _lastWorkerHitIndex = index;

        AudioClip clip = workerHitClips[index];
        if (clip != null)
            oneShotSource.PlayOneShot(clip);
    }

    private void HandleWorkerRecovered()
    {
        if (workerRecoveredClip != null)
            oneShotSource.PlayOneShot(workerRecoveredClip);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private void StartEngineIdle()
    {
        if (engineSource == null || engineIdle == null) return;
        engineSource.clip  = engineIdle;
        engineSource.loop  = true;
        engineSource.pitch = enginePitchIdle;
        engineSource.Play();
    }

    private void SwapEngineClip(AudioClip newClip)
    {
        if (newClip == null || engineSource.clip == newClip) return;
        engineSource.clip = newClip;
        engineSource.loop = true;
        engineSource.Play();
    }

    private void ValidateSources()
    {
        if (engineSource  == null) Debug.LogWarning("[ForkliftAudioManager] engineSource is not assigned.");
        if (forkSource    == null) Debug.LogWarning("[ForkliftAudioManager] forkSource is not assigned.");
        if (oneShotSource == null) Debug.LogWarning("[ForkliftAudioManager] oneShotSource is not assigned.");
    }
}