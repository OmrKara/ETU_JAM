using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 20f;
    public Rigidbody2D rb;
    float horizontal;
    float vertical;
    public Animator an;
    public int facingDirection = 1;
    
    

    void FixedUpdate()
    {
        horizontal = (Keyboard.current.dKey.isPressed ? 1f : 0f) + (Keyboard.current.aKey.isPressed ? -1f : 0f);
        vertical = (Keyboard.current.wKey.isPressed ? 1f : 0f) + (Keyboard.current.sKey.isPressed ? -1f : 0f);
        rb.linearVelocity = new Vector2(horizontal, vertical) * speed;

        an.SetFloat("horizontal", Mathf.Abs(horizontal));
        an.SetFloat("vertical", Mathf.Abs(vertical));

        if (horizontal < 0 && transform.localScale.x == 1 || horizontal > 0 && transform.localScale.x == -1) {
            facingDirection *= -1;
            transform.localScale = new Vector3 (transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }
        




    }
}
