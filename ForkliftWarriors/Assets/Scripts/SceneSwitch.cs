using UnityEngine;

public class SceneSwitch : MonoBehaviour
{
    public void SwitchScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}