using Unity.VisualScripting;
using UnityEngine;

public class SharkScript : MonoBehaviour
{

    public enum Facing
    {
        Left,
        Right
    }

    private float random;
    private float range => (facing == Facing.Left) ? -random : random;
    private float posx => gameObject.transform.position.x + range;
    [SerializeField] private float velocity;

    private Facing facing;

    void Start()
    {
        random = Random.Range(4, 8);
        facing = Facing.Left;
    }
    void Update()
    {
        gameObject.GetComponent<Rigidbody2D>().linearVelocityX = (facing == Facing.Left) ? -velocity : velocity;
        if (facing == Facing.Left && gameObject.transform.position.x < range)
        {
            random = Random.Range(4, 8);
            facing = (facing == Facing.Left) ? Facing.Right : Facing.Left;
            //bool b = gameObject.GetComponentInChildren<SpriteRenderer>().flipX;
            gameObject.GetComponentInChildren<SpriteRenderer>().flipX = !gameObject.GetComponentInChildren<SpriteRenderer>().flipX;
        }
        else if (facing == Facing.Right && gameObject.transform.position.x > range)
        {
            random = Random.Range(4, 8);
            facing = (facing == Facing.Left) ? Facing.Right : Facing.Left;
            //bool b = gameObject.GetComponentInChildren<SpriteRenderer>().flipX;
            gameObject.GetComponentInChildren<SpriteRenderer>().flipX = !gameObject.GetComponentInChildren<SpriteRenderer>().flipX;
        }
    }
}
