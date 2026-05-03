// ForkliftEvents.cs
// Central event bus for the forklift system.
// No MonoBehaviour needed — pure static C# events.
// Any script can fire events without needing a reference to AudioManager.

public static class ForkliftEvents
{
    public static event System.Action<float> OnForkliftSpeedChanged;
    public static event System.Action OnForkliftBraked;

 
    public static event System.Action<float> OnForkRaising;


    public static event System.Action<float> OnForkSliding;

    public static event System.Action<float> OnForkTilting;

    public static event System.Action OnLeverGrabbed;

    public static event System.Action OnLeverReleased;

    public static event System.Action OnPalletSnapped;

    public static event System.Action OnPalletDropped;

    public static event System.Action OnPalletPlaced;
    public static event System.Action<int> OnScoreUpdated;

    public static event System.Action OnWorkerHit;

    public static event System.Action OnWorkerRecovered;


    public static void RaiseForkliftSpeed(float normalisedSpeed) => OnForkliftSpeedChanged?.Invoke(normalisedSpeed);
    public static void RaiseForkliftBraked() => OnForkliftBraked?.Invoke();

    public static void RaiseForkRaising(float input) => OnForkRaising?.Invoke(input);
    public static void RaiseForkSliding(float input) => OnForkSliding?.Invoke(input);
    public static void RaiseForkTilting(float input) => OnForkTilting?.Invoke(input);
    public static void RaiseLeverGrabbed() => OnLeverGrabbed?.Invoke();
    public static void RaiseLeverReleased() => OnLeverReleased?.Invoke();

    public static void RaisePalletSnapped() => OnPalletSnapped?.Invoke();
    public static void RaisePalletDropped() => OnPalletDropped?.Invoke();

    public static void RaisePalletPlaced() => OnPalletPlaced?.Invoke();
    public static void RaiseScoreUpdated(int score) => OnScoreUpdated?.Invoke(score);

    public static void RaiseWorkerHit() => OnWorkerHit?.Invoke();
    public static void RaiseWorkerRecovered() => OnWorkerRecovered?.Invoke();
}
