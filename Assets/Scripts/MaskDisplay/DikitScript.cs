using UnityEngine;

public class DikitScript : MonoBehaviour
{

    [SerializeField] private Transform spawnPoint;
    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject player = collision.gameObject;

        if (player.CompareTag("Player"))
        {
            PlayerMovement2D.i.gameObject.transform.position = spawnPoint.position;
            SoundManager.PlaySound(SoundManager.Sound.PlayerDie);
        }
    }
}
