using UnityEngine;

public class SharkScript : MonoBehaviour
{
    void Update()
    {
        gameObject.GetComponent<Rigidbody2D>().linearVelocityX = -2;
        if (gameObject.transform.position.x < -10)
        {
            gameObject.SetActive(false);
        }
    }
}
