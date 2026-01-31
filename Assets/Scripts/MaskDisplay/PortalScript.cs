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
            /*if (enteredObject.GetComponent<InventoryManager>().isIN.Length == levelMaskAmount)
            {
                Debug.Log("PORTALLLLLLLL");
                
            }*/
            SceneManager.LoadScene(levelNum + 1);
            SoundManager.PlaySound(SoundManager.Sound.PortalSound, transform.position);
        }
    }

}
