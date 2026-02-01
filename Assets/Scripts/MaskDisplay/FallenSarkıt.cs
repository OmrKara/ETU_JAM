using System.Collections;
using UnityEngine;

public class FallingPlatform2D : MonoBehaviour
{
    [Header("Shake")]
    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.03f;

    [Header("Fall")]
    public float startDelayAfterShake = 0.05f;
    public float acceleration = 25f;
    public float maxSpeed = 20f;

    [Header("Cleanup")]
    public bool destroyAfterFalling = true;
    public float destroyAfterSeconds = 3f;

    bool triggered;
    bool isFalling;
    float velocity;
    Vector3 basePos;
    Coroutine routine;

    void Awake()
    {
        basePos = transform.position;
    }

    public void TriggerFall()
    {
        if (triggered) return;
        triggered = true;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShakeThenFall());
    }

    IEnumerator ShakeThenFall()
    {
        float t = 0f;
        SoundManager.PlaySound(SoundManager.Sound.FallenSarkýtTitreme);

        while (t < shakeDuration)
        {
            Vector2 rnd = Random.insideUnitCircle * shakeStrength;
            transform.position = basePos + (Vector3)rnd;

            t += Time.deltaTime;
            yield return null;
        }

        transform.position = basePos;

        if (startDelayAfterShake > 0f)
            yield return new WaitForSeconds(startDelayAfterShake);

        isFalling = true;
        SoundManager.PlaySound(SoundManager.Sound.FallenSarkýtDüþme);

        if (destroyAfterFalling)
            Destroy(gameObject, destroyAfterSeconds);
    }

    void Update()
    {
        if (!isFalling) return;

        velocity += acceleration * Time.deltaTime;
        velocity = Mathf.Min(velocity, maxSpeed);

        transform.position += Vector3.down * velocity * Time.deltaTime;
        basePos = transform.position;
    }
}
