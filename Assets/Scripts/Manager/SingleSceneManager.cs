using UnityEngine;
using UnityEngine.SceneManagement;

public class SingleSceneManager : MonoBehaviour
{
    public static void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public static void LoadActiveLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void LoadNextScene()
    {
        int lastSceneIndex = SceneManager.sceneCountInBuildSettings - 1;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex > lastSceneIndex)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
