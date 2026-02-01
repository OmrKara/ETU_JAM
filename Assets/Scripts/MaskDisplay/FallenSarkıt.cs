using UnityEngine;

public class FallenSarkıt : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;

        if (obj.CompareTag("Player"))
        {
            PlayerMovement2D.i.gameObject.transform.position = spawnPoint.position;
        }
    }
}
