using UnityEngine;

public class DikitScript : MonoBehaviour
{

    [SerializeField] private Transform spawnPoint;
    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject player = collision.gameObject;

        if (player.CompareTag("Player"))
        {
            player.transform.position = spawnPoint.position;
        }
    }
}
