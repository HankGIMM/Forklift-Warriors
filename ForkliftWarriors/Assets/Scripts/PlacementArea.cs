using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class PlacementArea : MonoBehaviour
{
    private Rigidbody boxPallet;
    private bool isSnapped;
    public GameObject snapPoint;
    public GameObject objectiveAreaVisuals;
    private GameObject scoreObject;
    private ScoreScript scoreScript;
    void Start()
    {
        scoreObject = GameObject.FindGameObjectWithTag("ScoreManager");
        scoreScript = scoreObject.GetComponent<ScoreScript>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root != other.transform && other.CompareTag("boxpallet"))
            return;

        if (other.CompareTag("boxpallet") && !isSnapped)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                objectiveAreaVisuals.SetActive(false);
                //play animation like confetti or something
                Snap(other.transform, rb);
            }
        }
    }

    private void Snap(Transform root, Rigidbody rb)
    {
        boxPallet = rb;
        boxPallet.isKinematic = true;
        root.SetParent(snapPoint.transform);
        root.position = snapPoint.transform.position;
        root.rotation = snapPoint.transform.rotation;
        isSnapped = true;

       
        foreach (Collider col in root.GetComponentsInChildren<Collider>())
        {
            col.enabled = false; 
        }
        Debug.Log("Snapped permanently: " + root.name);
        scoreScript.ScoreUptaded();
    }
}