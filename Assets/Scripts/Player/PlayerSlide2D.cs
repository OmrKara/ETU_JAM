using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSlide2D : MonoBehaviour
{
    public static PlayerSlide2D Instance;

    [Header("References")]
    [SerializeField] private PlayerMovement2D movement; // inspector'dan bağla (fallback var)
    [SerializeField] private Behaviour[] disableDuringSlide;

    [Header("Slide Settings")]
    [SerializeField] private float slideSpeed = 14f;
    [SerializeField] private float slideDuration = 0.18f;
    [SerializeField] private float slideCooldown = 0.35f;

    [Tooltip("Slide sadece yerdeyken başlasın (buffer ile landing anında başlayabilir).")]
    [SerializeField] private bool groundOnly = true;

    [Header("Slide Buffer")]
    [Tooltip("Havadayken slide isteği gelirse bu kadar süre sakla. Landing olunca uygula. 0 => kapalı.")]
    [SerializeField] private float slideBufferTime = 0.12f;

    [Header("Direction")]
    [SerializeField] private float inputDeadzone = 0.05f;

    [Header("Velocity Handling")]
    [SerializeField] private bool zeroYVelocityOnStart = false;
    [SerializeField] private bool zeroXVelocityOnEnd = false;

    private Rigidbody2D rb;
    private bool isSliding;
    private float nextSlideTime;

    // buffer internal
    private float slideBufferTimer;
    private int bufferedDir;

    private bool wasGrounded;

    public bool IsSliding => isSliding;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();

        if (movement == null)
            movement = GetComponent<PlayerMovement2D>(); // fallback
    }

    private void Update()
    {
        if (isSliding) return;

        if (movement == null) return;

        var input = InputController2D.Current as InputController2D;
        if (input == null) return;

        // landing anını yakala
        bool grounded = movement.IsGrounded;
        bool justLanded = (!wasGrounded && grounded);
        wasGrounded = grounded;

        // cooldown
        if (Time.time < nextSlideTime)
        {
            // buffer süre azalsın (cooldown sırasında bile istersen tutabiliriz; ben azaltıyorum)
            if (slideBufferTimer > 0f) slideBufferTimer -= Time.deltaTime;
            return;
        }

        // 1) SlideRequested geldiyse:
        if (input.SlideRequested)
        {
            int dir = ResolveSlideDirection(input.Move);

            if (!groundOnly)
            {
                // groundOnly kapalıysa direkt başlat
                StartCoroutine(SlideRoutine(dir));
                return;
            }

            if (grounded)
            {
                // yerdeyse direkt
                StartCoroutine(SlideRoutine(dir));
                return;
            }

            // yerde değilse buffer'a al
            if (slideBufferTime > 0f)
            {
                slideBufferTimer = slideBufferTime;
                bufferedDir = dir;
            }
        }

        // 2) Buffer aktifken landing olursa slide başlat
        if (groundOnly && slideBufferTimer > 0f)
        {
            slideBufferTimer -= Time.deltaTime;

            if (justLanded)
            {
                StartCoroutine(SlideRoutine(bufferedDir));
                slideBufferTimer = 0f;
            }
        }
    }

    private int ResolveSlideDirection(Vector2 move)
    {
        if (Mathf.Abs(move.x) > inputDeadzone)
            return move.x > 0 ? 1 : -1;

        return (movement != null && movement.IsFacingRight) ? 1 : -1;
    }

    private IEnumerator SlideRoutine(int dir)
    {
        isSliding = true;
        nextSlideTime = Time.time + slideCooldown;

        // movement'ı garanti kapat
        if (movement != null) movement.enabled = false;

        // diğer scriptleri kapat
        if (disableDuringSlide != null)
            foreach (var b in disableDuringSlide)
                if (b != null) b.enabled = false;

        // slide başı
        if (zeroYVelocityOnStart)
        {
            var v0 = rb.linearVelocity;
            v0.y = 0f;
            rb.linearVelocity = v0;
        }

        float t = 0f;
        while (t < slideDuration)
        {
            // Gravity'nin devam etmesi için Y'ye dokunmuyoruz
            rb.linearVelocity = new Vector2(dir * slideSpeed, rb.linearVelocity.y);

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // slide bitiş
        var v = rb.linearVelocity;
        if (zeroXVelocityOnEnd) v.x = 0f;
        rb.linearVelocity = v;

        // scriptleri geri aç
        if (disableDuringSlide != null)
            foreach (var b in disableDuringSlide)
                if (b != null) b.enabled = true;

        if (movement != null) movement.enabled = true;

        isSliding = false;
    }
}
