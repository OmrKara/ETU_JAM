using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonListener : MonoBehaviour
{
    public void changeScene()
    {
        SceneManager.LoadScene("LevelOne");
    }

    public void ReturnStartScreen()
    {
        SceneManager.LoadScene(0);
    }
}
