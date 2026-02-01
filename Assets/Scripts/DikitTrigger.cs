using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    public FallingPlatform2D platform;


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            platform.TriggerFall();
        }
    }
}