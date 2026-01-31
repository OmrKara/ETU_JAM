using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class CollisionScript : MonoBehaviour
{
    public string text;
    public Text textBar;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            textBar.text = text;
        }
    }

}
