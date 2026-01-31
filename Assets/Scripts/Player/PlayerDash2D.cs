using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash2D : MonoBehaviour
{
    public static PlayerDash2D Instance;
    private PlayerMovement2D movement;
    [Header("References")]

    [Tooltip("Dash s�ras�nda kapat�lacak scriptler. (PlayerMovement2D mutlaka burada olsun)")]
    [SerializeField] private Behaviour[] disableDuringDash;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashCooldown = 0.25f;

    [Header("Direction")]

    [Tooltip("Move.x bunun alt�ndaysa 'input yok' say�l�r.")]
    [SerializeField] private float inputDeadzone = 0.05f;

    [Header("Physics During Dash")]
    [Tooltip("Dash s�ras�nda yer�ekimi tamamen kapal�. (rb.gravityScale=0)")]
    [SerializeField] private bool disableGravityDuringDash = true;

    [Tooltip("Dash ba��nda y h�z�n� s�f�rla (z�plama etkisi ta��nmas�n).")]
    [SerializeField] private bool zeroYVelocityOnStart = true;

    [Tooltip("Dash bitince X h�z�n� s�f�rla (istersen false yap).")]
    [SerializeField] private bool zeroXVelocityOnEnd = true;

    private Rigidbody2D rb;

    private bool isDashing;
    private float nextDashTime;

    private float savedGravityScale;

    // Y�n haf�zas�
    private int lastFacing = 1; // +1 right, -1 left

    public bool IsDashing => isDashing;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        movement = PlayerMovement2D.i;

    }

    private void Update()
    {
        if (isDashing) return;
        if (Time.time < nextDashTime) return;

        // Input kayna�� (senin singleton)
        var input = InputController2D.Current as InputController2D;
        if (input == null) return;

        // Dash iste�i geldiyse ba�lat
        if (input.DashRequested)
        {
            // 🚫 Crouch / stand-lock varken dash yok
            if (movement != null && movement.IsCrouching)
                return;

            int dir = ResolveDashDirection(input.Move);
            StartCoroutine(DashRoutine(dir));

            SoundManager.PlaySound(SoundManager.Sound.PlayerDash);
        }

    }

    private int ResolveDashDirection(Vector2 move)
    {
        if (Mathf.Abs(move.x) > 0.05f)
            return move.x > 0 ? 1 : -1;

        return PlayerMovement2D.i.IsFacingRight ? 1 : -1;
    }

    private IEnumerator DashRoutine(int dir)
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        // Davran��lar� kapat (movement/jump/anim controller vs.)
        if (disableDuringDash != null)
        {
            for (int i = 0; i < disableDuringDash.Length; i++)
            {
                if (disableDuringDash[i] != null)
                    disableDuringDash[i].enabled = false;
            }
        }

        // Gravity ayarlar�n� kaydet
        savedGravityScale = rb.gravityScale;
        if (disableGravityDuringDash)
            rb.gravityScale = 0f;

        // Dash ba�lang�c�: h�zlar� temizle
        if (zeroYVelocityOnStart)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Dash boyunca sabit h�z uygula (physics step ile)
        float t = 0f;
        while (t < dashDuration)
        {
            rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Dash biti�
        Vector2 v = rb.linearVelocity;

        if (zeroXVelocityOnEnd)
            v.x = 0f;

        // y'yi 0 tutmak istiyorsun (dash s�ras�nda z�plama yok). Bitince d����e izin ver.
        // gravity eski haline d�n�nce zaten d��ecek.
        v.y = 0f;
        rb.linearVelocity = v;

        // Gravity geri y�kle
        rb.gravityScale = savedGravityScale;

        // Davran��lar� geri a�
        if (disableDuringDash != null)
        {
            for (int i = 0; i < disableDuringDash.Length; i++)
            {
                if (disableDuringDash[i] != null)
                    disableDuringDash[i].enabled = true;
            }
        }

        isDashing = false;
    }
}
