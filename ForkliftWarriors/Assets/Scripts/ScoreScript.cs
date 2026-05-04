using UnityEngine;
using TMPro;

public class ScoreScript : MonoBehaviour
{
    public int score = 1;
    public TextMeshProUGUI scoreText;
    public LevelManager levelManager;

    public void ScoreUpdated()
    {
        score += 1;
        scoreText.text = "Level " + score.ToString();
        levelManager.OnScoreChanged(score);
        if(score == 6)
        {
            scoreText.text = "You Win!";
        }
    }
}