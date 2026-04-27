using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private GameObject scoreObject;
    private ScoreScript scoreScript;

    public GameObject[] palletObjects;
    public GameObject[] placementAreas;
    void Start()
    {
        scoreObject = GameObject.FindGameObjectWithTag("ScoreManager");
        scoreScript = scoreObject.GetComponent<ScoreScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (scoreScript.score == 2)
        {
            palletObjects[0].SetActive(true);
            placementAreas[0].SetActive(true);
        }
        else if(scoreScript.score == 3)
        {
            palletObjects[1].SetActive(true);
            placementAreas[1].SetActive(true);
        }
        else if(scoreScript.score == 4)
        {
            palletObjects[2].SetActive(true);
            placementAreas[2].SetActive(true);
        }
        else if(scoreScript.score == 5)
        {
            palletObjects[3].SetActive(true);
            placementAreas[3].SetActive(true);
        }
    }
}
