using UnityEngine;

public class SharkScript : MonoBehaviour
{
    Random random;
    void Awake()
    {

    }

    void Start()
    {
        random = new Random();
    }
    void Update()
    {
        gameObject.GetComponent<Rigidbody2D>().linearVelocityX = -2;
        if (gameObject.transform.position.x < -10)
        {
            gameObject.SetActive(false);
        }
    }
}
