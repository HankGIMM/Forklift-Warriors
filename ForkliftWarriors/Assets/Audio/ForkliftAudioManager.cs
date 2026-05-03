// ForkliftAudioManager.cs
// Attach this to a dedicated "AudioManager" GameObject in your scene.
// Wire up AudioSources and AudioClips in the Inspector.
// This script knows nothing about game logic — it only listens to ForkliftEvents.

using UnityEngine;

public class ForkliftAudioManager : MonoBehaviour
{

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
    [Tooltip("Low rumble played when the forklift is stationary.")]
    [SerializeField] private AudioClip engineIdle;

    [Tooltip("Higher-energy clip played while moving.")]
    [SerializeField] private AudioClip engineDrive;

    [Tooltip("Short squeal / brake clip played when the forklift stops.")]
    [SerializeField] private AudioClip brakeSound;

    // ------------------------------------------------------------------
    // Fork Clips
    // ------------------------------------------------------------------

    [Header("Fork Clips")]
    [Tooltip("Hydraulic whine played while the fork is rising or lowering.")]
    [SerializeField] private AudioClip forkRaiseClip;

    [Tooltip("Lateral slide squeak while the fork moves left / right.")]
    [SerializeField] private AudioClip forkSlideClip;

    [Tooltip("Tilt mechanism groan while the fork tilts.")]
    [SerializeField] private AudioClip forkTiltClip;

    // ------------------------------------------------------------------
    // Lever Clips
    // ------------------------------------------------------------------

    [Header("Lever Clips")]
    [Tooltip("Click / thunk when a lever is grabbed.")]
    [SerializeField] private AudioClip leverGrabClip;

    [Tooltip("Click / thunk when a lever is released.")]
    [SerializeField] private AudioClip leverReleaseClip;

    // ------------------------------------------------------------------
    // Pallet Clips
    // ------------------------------------------------------------------

    [Header("Pallet Clips")]
    [Tooltip("Metal clunk when a pallet locks onto the fork.")]
    [SerializeField] private AudioClip palletSnapClip;

    [Tooltip("Thud / scrape when the pallet is dropped from the fork.")]
    [SerializeField] private AudioClip palletDropClip;

    [Tooltip("Satisfying placement sound when the pallet hits its objective area.")]
    [SerializeField] private AudioClip palletPlacedClip;

    // ------------------------------------------------------------------
    // Score / UI Clips
    // ------------------------------------------------------------------

    [Header("Score / UI Clips")]
    [Tooltip("Jingle or chime when the score / level increments.")]
    [SerializeField] private AudioClip scoreUpClip;

    // ------------------------------------------------------------------
    // Worker Clips
    // ------------------------------------------------------------------

    [Header("Worker Clips")]
    [Tooltip("Impact sound when a worker is knocked into ragdoll.")]
    [SerializeField] private AudioClip workerHitClip;

    [Tooltip("Groan or shuffle when a worker recovers from ragdoll.")]
    [SerializeField] private AudioClip workerRecoveredClip;

    // ------------------------------------------------------------------
    // Engine tuning
    // ------------------------------------------------------------------

    [Header("Engine Tuning")]
    [Tooltip("Engine pitch at zero speed.")]
    [SerializeField] private float enginePitchIdle = 0.8f;

    [Tooltip("Engine pitch at full speed.")]
    [SerializeField] private float enginePitchMax = 1.6f;

    // Tracks whether the engine is currently in drive mode so we only
    // swap clips when the state actually changes (avoids click artifacts).
    private bool _wasMoving = false;

    // Tracks whether fork sources are playing so we stop them cleanly
    // the frame input drops to zero.
    private bool _forkRaiseActive = false;
    private bool _forkSlideActive = false;
    private bool _forkTiltActive  = false;


    // Lifecycle


    void Awake()
    {
        ValidateSources();
        StartEngineIdle();
    }

    void OnEnable()
    {
        ForkliftEvents.OnForkliftSpeedChanged += HandleForkliftSpeed;
        ForkliftEvents.OnForkliftBraked       += HandleBrake;

        ForkliftEvents.OnForkRaising  += HandleForkRaise;
        ForkliftEvents.OnForkSliding  += HandleForkSlide;
        ForkliftEvents.OnForkTilting  += HandleForkTilt;
        ForkliftEvents.OnLeverGrabbed += HandleLeverGrabbed;
        ForkliftEvents.OnLeverReleased += HandleLeverReleased;

        ForkliftEvents.OnPalletSnapped += HandlePalletSnapped;
        ForkliftEvents.OnPalletDropped += HandlePalletDropped;
        ForkliftEvents.OnPalletPlaced  += HandlePalletPlaced;

        ForkliftEvents.OnScoreUpdated  += HandleScoreUpdated;

        ForkliftEvents.OnWorkerHit      += HandleWorkerHit;
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


    // Engine handlers



    // Called every FixedUpdate from Forklift.cs with a normalised speed value.
    //Swaps between idle/drive clips and pitch-shifts to match speed.
    private void HandleForkliftSpeed(float normalisedSpeed)
    {
        bool isMoving = Mathf.Abs(normalisedSpeed) > 0.05f;

        // Pitch-shift the engine relative to speed regardless of clip
        engineSource.pitch = Mathf.Lerp(enginePitchIdle, enginePitchMax, Mathf.Abs(normalisedSpeed));

        if (isMoving && !_wasMoving)
        {
            // Transitioned from idle to driving — swap clip
            SwapEngineClip(engineDrive);
            _wasMoving = true;
        }
        else if (!isMoving && _wasMoving)
        {
            // Transitioned from driving to idle
            SwapEngineClip(engineIdle);
            _wasMoving = false;
        }
    }

    private void HandleBrake()
    {
        if (brakeSound != null)
            oneShotSource.PlayOneShot(brakeSound);
    }


    // Fork handlers


    /// <summary>
    /// Called every frame the raise lever has input. Starts / stops the looping
    /// hydraulic clip and adjusts pitch to reflect speed of movement.
    /// </summary>
    private void HandleForkRaise(float input)
    {
        HandleForkLoop(ref _forkRaiseActive, forkRaiseClip, input);
    }

    private void HandleForkSlide(float input)
    {
        HandleForkLoop(ref _forkSlideActive, forkSlideClip, input);
    }

    private void HandleForkTilt(float input)
    {
        HandleForkLoop(ref _forkTiltActive, forkTiltClip, input);
    }

    /// <summary>
    /// Reusable helper: starts the fork looping source when input is non-zero,
    /// stops it when input drops to zero.
    /// Note: forkSource can only play one clip at a time — if you need all three
    /// fork sounds simultaneously, add extra AudioSource fields and pass them in.
    /// </summary>
    private void HandleForkLoop(ref bool activeFlag, AudioClip clip, float input)
    {
        bool hasInput = Mathf.Abs(input) > 0.05f;

        if (hasInput && !activeFlag)
        {
            if (clip != null)
            {
                forkSource.clip = clip;
                forkSource.loop = true;
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
            // Already playing — just update pitch in real time
            forkSource.pitch = Mathf.Lerp(0.9f, 1.2f, Mathf.Abs(input));
        }
    }

    private void HandleLeverGrabbed()
    {
        if (leverGrabClip != null)
            oneShotSource.PlayOneShot(leverGrabClip);
    }

    private void HandleLeverReleased()
    {
        if (leverReleaseClip != null)
            oneShotSource.PlayOneShot(leverReleaseClip);
    }


    // Pallet handlers


    private void HandlePalletSnapped()
    {
        if (palletSnapClip != null)
            oneShotSource.PlayOneShot(palletSnapClip);
    }

    private void HandlePalletDropped()
    {
        if (palletDropClip != null)
            oneShotSource.PlayOneShot(palletDropClip);
    }

    private void HandlePalletPlaced()
    {
        if (palletPlacedClip != null)
            oneShotSource.PlayOneShot(palletPlacedClip);
    }


    // Score / UI handlers
 

    private void HandleScoreUpdated(int newScore)
    {
        if (scoreUpClip != null)
            oneShotSource.PlayOneShot(scoreUpClip);
    }


    // Worker handlers
  

    private void HandleWorkerHit()
    {
        if (workerHitClip != null)
            oneShotSource.PlayOneShot(workerHitClip);
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
        engineSource.clip = engineIdle;
        engineSource.loop = true;
        engineSource.pitch = enginePitchIdle;
        engineSource.Play();
    }

    /// <summary>Swaps the engine clip without a hard stop, minimising audio clicks.</summary>
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