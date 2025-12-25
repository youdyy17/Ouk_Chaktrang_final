using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneSwitcher : MonoBehaviour
{
    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}