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

        if (reachToggle != null)
        {
            reachToggle.onValueChanged.AddListener(OnToggleChanged);
            ApplyReach(reachToggle.isOn);
        }
        else
        {
            Debug.LogWarning("Reach Toggle is not assigned!");
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        Debug.Log("Reach toggle changed: " + isOn);
        ApplyReach(isOn);
    }

    public void ApplyReach(bool extended)
    {
        float distance = extended ? extendedReach : normalReach;

        SetFarCasterDistance(leftInteractor, distance);
        SetFarCasterDistance(rightInteractor, distance);
    }

    private void SetFarCasterDistance(NearFarInteractor interactor, float distance)
    {
        if (interactor == null)
        {
            Debug.LogWarning("Interactor is null!");
            return;
        }

        bool applied = false;

        
        var curveCaster = interactor.GetComponentInChildren<CurveInteractionCaster>(true);
        if (curveCaster != null)
        {
            curveCaster.castDistance = distance;
            Debug.Log($"{interactor.name} → CurveInteractionCaster set to {distance}");
            applied = true;
        }

       
        var rayInteractor = interactor.GetComponentInChildren<XRRayInteractor>(true);
        if (rayInteractor != null)
        {
            rayInteractor.maxRaycastDistance = distance;
            Debug.Log($"{interactor.name} → XRRayInteractor set to {distance}");
            applied = true;
        }

        if (!applied)
        {
            Debug.LogWarning($"No valid caster found on {interactor.name}");

            // 🔍 Debug: list components to help you identify what's actually there
            foreach (var comp in interactor.GetComponentsInChildren<Component>(true))
            {
                Debug.Log($"{interactor.name} has component: {comp.GetType()}");
            }
        }

        // 🔄 Force refresh (some XR setups cache values)
        interactor.enabled = false;
        interactor.enabled = true;
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

        Debug.Log($"AutoFind → Left: {leftInteractor?.name}, Right: {rightInteractor?.name}");
    }
}