using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalScript : MonoBehaviour
{

    [SerializeField] public int levelMaskAmount = 3;
    public int levelNum;


    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject enteredObject = collision.gameObject;

        if (enteredObject.CompareTag("Player"))
        {
            if (GameHandler.I.ownedItems.Count == levelMaskAmount)
            {
                SceneManager.LoadScene(levelNum + 1);
                SoundManager.PlaySound(SoundManager.Sound.PortalSound, transform.position);
            }
            
        }
    }

}
