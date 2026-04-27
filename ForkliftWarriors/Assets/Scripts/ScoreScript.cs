using UnityEngine;
using TMPro;

public class ScoreScript : MonoBehaviour
{
    public int score = 1;
    public TextMeshProUGUI scoreText;

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ScoreUptaded()
    {
        score += 1;
        scoreText.text = "Level " + score.ToString();
    }
}
