using UnityEngine;

public class SarkıtScript : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;

        if (obj.CompareTag("Player"))
        {
        }
    }
}
