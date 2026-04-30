using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UnityEngine.UI;

public class RaycastReachController : MonoBehaviour
{
    [Header("Near Far Interactors")]
    public NearFarInteractor leftInteractor;
    public NearFarInteractor rightInteractor;

    [Header("Reach Presets")]
    public float normalReach = 5f;
    public float extendedReach = 15f;

    [Header("UI")]
    public Toggle reachToggle;

    private void Start()
    {
        if (leftInteractor == null || rightInteractor == null)
            AutoFindInteractors();

        reachToggle.onValueChanged.AddListener(OnToggleChanged);
        ApplyReach(reachToggle.isOn);
    }

    private void OnToggleChanged(bool isOn)
    {
        ApplyReach(isOn);
    }

    private void ApplyReach(bool extended)
    {
        float distance = extended ? extendedReach : normalReach;
        SetFarCasterDistance(leftInteractor, distance);
        SetFarCasterDistance(rightInteractor, distance);
    }

    private void SetFarCasterDistance(NearFarInteractor interactor, float distance)
{
    if (interactor == null) return;

    if (interactor.farInteractionCaster is CurveInteractionCaster caster)
        caster.castDistance = distance;
    else
        Debug.LogWarning($"Could not cast farInteractionCaster on {interactor.name} to CurveInteractionCaster.");
}

    private void AutoFindInteractors()
    {
        NearFarInteractor[] interactors = FindObjectsByType<NearFarInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in interactors)
        {
            string nameLower = interactor.gameObject.name.ToLower();
            if (nameLower.Contains("left"))
                leftInteractor = interactor;
            else if (nameLower.Contains("right"))
                rightInteractor = interactor;
        }
    }
}