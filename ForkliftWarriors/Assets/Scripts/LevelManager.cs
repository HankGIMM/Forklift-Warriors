using UnityEngine;

   public class LevelManager : MonoBehaviour
{
    public ScoreScript scoreScript;
    public GameObject[] palletObjects;
    public GameObject[] placementAreas;

    public void OnScoreChanged(int score)
    {
        // score - 2 gives us the correct array index (score 2 = index 0, etc.)
        int index = score - 2;
        if (index >= 0 && index < palletObjects.Length)
        {
            palletObjects[index].SetActive(true);
            placementAreas[index].SetActive(true);
        }
    }
}

