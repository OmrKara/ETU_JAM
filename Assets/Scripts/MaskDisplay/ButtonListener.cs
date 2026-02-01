using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonListener : MonoBehaviour
{
    public void changeScene()
    {
        SceneManager.LoadScene("LevelTutorial");
    }

    public void ReturnStartScreen()
    {
        SceneManager.LoadScene(0);
    }
    public void Loader(int i)
    {
        SceneManager.LoadScene(i);
    }

    public void resumeGame()
    {
        GameHandler.I.TogglePause();
    }
}
