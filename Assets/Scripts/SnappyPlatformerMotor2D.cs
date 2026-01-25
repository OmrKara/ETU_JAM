using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SnappyPlatformerMotor2D : MonoBehaviour
{
    public static SnappyPlatformerMotor2D i;

    [Header("Input (IPlayerInput2D)")]
    [SerializeField] private MonoBehaviour inputBehaviour;
    private IPlayerInput2D input;

    [Header("Horizontal")]
    public float maxSpeed = 8.5f;
    public float groundAccel = 90f;
    public float groundDecel = 110f;
    public float airAccel = 70f;
    public float airDecel = 40f;

    [Header("Jump (feel)")]
    [Tooltip("Zıplama yüksekliğini belirleyen başlangıç dikey hızı.")]
    public float jumpVelocity = 14.5f;

    [Tooltip("Yukarı çıkarken uygulanan yerçekimi (küçük => daha floaty).")]
    public float gravityUp = 28f;

    [Tooltip("Düşerken uygulanan yerçekimi (büyük => daha hızlı düşer, floaty gider).")]
    public float gravityDown = 55f;

    [Tooltip("Jump tuşunu erken bırakırsan ekstra aşağı itiş (variable height).")]
    public float jumpCutGravityBonus = 65f;

    [Tooltip("Tepeye yakın küçük bir band; çok kısa Apex hissi için.")]
    public float apexBand = 1.0f;

    [Tooltip("Tepeye yaklaşınca çok az kontrol hissi için gravityUp'ı biraz artır.")]
    public float apexGravityBonus = 10f;

    [Header("Limits")]
    public float maxFallSpeed = 22f;   // terminal velocity
    public float coyoteTime = 0.10f;
    public float jumpBuffer = 0.10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.7f, 0.15f);
    public LayerMask groundLayer;

    // Public read-only
    public bool IsGrounded => isGrounded;
    public Rigidbody2D Rb => rb;

    private Rigidbody2D rb;

    private bool isGrounded;
    private float coyoteTimer;
    private float bufferTimer;

    private void Awake()
    {
        i = this;
        rb = GetComponent<Rigidbody2D>();

        // ✅ Biz yöneteceğiz: Unity gravity kapalı
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        input = inputBehaviour as IPlayerInput2D;
        if (input == null) input = InputController2D.Current;
    }

    private void Update()
    {
        if (input == null) return;

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        // Timers
        coyoteTimer = isGrounded ? coyoteTime : coyoteTimer - Time.deltaTime;

        if (input.JumpPressed) bufferTimer = jumpBuffer;
        else bufferTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (input == null) return;

        Vector2 v = rb.linearVelocity;

        // ---------- Horizontal (snappy) ----------
        float targetX = input.Move.x * maxSpeed;
        float accel = isGrounded
            ? (Mathf.Abs(targetX) > 0.01f ? groundAccel : groundDecel)
            : (Mathf.Abs(targetX) > 0.01f ? airAccel : airDecel);

        v.x = Mathf.MoveTowards(v.x, targetX, accel * Time.fixedDeltaTime);

        // ---------- Jump trigger ----------
        if (bufferTimer > 0f && coyoteTimer > 0f)
        {
            v.y = jumpVelocity;      // direkt velocity set => “snappy takeoff”
            bufferTimer = 0f;
            coyoteTimer = 0f;
            isGrounded = false;
        }

        // ---------- Custom gravity (kills floaty) ----------
        // 1) Yukarı çıkarken daha az, düşerken daha çok gravity
        float g = (v.y > 0f) ? gravityUp : gravityDown;

        // 2) Apex'e yaklaşınca (|vy| küçük) “asılı kalmasın” diye biraz daha gravity ekle
        if (Mathf.Abs(v.y) < apexBand)
            g += apexGravityBonus;

        // 3) Variable height: jump bırakınca ekstra aşağı çek (yukarı çıkış kısa olur)
        if (!input.JumpHeld && v.y > 0f)
            g += jumpCutGravityBonus;

        // gravity uygula
        v.y -= g * Time.fixedDeltaTime;

        // terminal velocity
        if (v.y < -maxFallSpeed) v.y = -maxFallSpeed;

        rb.linearVelocity = v;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
