using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMan : MonoBehaviour
{
    public static void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public static void LoadEndScene()
    {
        SceneManager.LoadScene("EndScene");
    }
    public static void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

}
