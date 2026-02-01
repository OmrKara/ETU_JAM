using UnityEngine;
using UnityEngine.SceneManagement;

public class ChickenScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Transform spawnPoint;

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<Rigidbody2D>().linearVelocityX = -5f;

        if (gameObject.transform.position.y < -10)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject enteredObject = collision.gameObject;

        if (enteredObject.CompareTag("Player"))
        {
            PlayerMovement2D.i.gameObject.transform.position = spawnPoint.position;
        }
    }
}
