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
    [SerializeField] private float velocity;

    private Facing facing;

    void Start()
    {
        random = Random.Range(8, 4);
        facing = Facing.Left;
    }
    void Update()
    {
        gameObject.GetComponent<Rigidbody2D>().linearVelocityX = (facing == Facing.Left) ? -velocity : velocity;
        if (gameObject.transform.position.x < range)
        {
            random = Random.Range(8, 4);
            facing = (facing == Facing.Left) ? Facing.Right : Facing.Left;
            //bool b = gameObject.GetComponentInChildren<SpriteRenderer>().flipX;
            gameObject.GetComponentInChildren<SpriteRenderer>().flipX = !gameObject.GetComponentInChildren<SpriteRenderer>().flipX;
        }
    }
}
