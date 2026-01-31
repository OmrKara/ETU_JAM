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
}
