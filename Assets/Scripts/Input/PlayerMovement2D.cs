using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement2D : MonoBehaviour
{
    public static PlayerMovement2D i;

    [Header("Input Source")]
    [SerializeField] private MonoBehaviour inputBehaviour;
    private IPlayerInput2D input;

    [Header("Horizontal")]
    public float maxWalkSpeed = 3;
    public float maxCrouchSpeed = 2;
    public float maxRunSpeed = 10f;
    private bool isSprinting; private bool isCrouching;


    public float groundAccel = 90f;
    public float groundDecel = 110f;
    public float airAccel = 70f;
    public float airDecel = 40f;

    [Header("Jump (snappy / anti-floaty)")]
    public float jumpVelocity = 15f;
    public float gravityUp = 32f;
    public float gravityDown = 70f;
    public float jumpCutGravityBonus = 90f;
    public float apexBand = 1.0f;
    public float apexGravityBonus = 12f;

    [Header("Limits")]
    public float maxFallSpeed = 26f;
    public float coyoteTime = 0.10f;
    public float jumpBuffer = 0.10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.7f, 0.15f);
    public LayerMask groundLayer;

    [Header("Facing / Flip")]
    [SerializeField] private Transform graphics; // child Graphics transform
    [SerializeField] private float faceDeadzone = 0.05f;

    private bool isFacingRight = true;
    private Vector3 graphicsBaseScale;


    [Header("Ledge Hop (airborne corner correction)")]
    [Tooltip("0 yaparsan kapalı. Havada köşe/ledge temasında verilecek minimum yukarı hız.")]
    public float ledgeHopVelocity = 2.2f;

    [Tooltip("Düşüşün başında da çalışsın istiyorsan, aşağı hız toleransı. Örn 2.5 => vy >= -2.5 iken izin verir.")]
    public float ledgeHopAllowedDownSpeed = 2.5f; // 0 yaparsan sadece ascending gibi olur

    [Tooltip("Üst bölge eşiğine ekstra tolerans (world unit). Pozitif => daha affedici (biraz daha aşağıdaki temasları da kabul eder).")]
    public float ledgeHopExtraTopOffset = 0.08f;


    [Tooltip("Contact noktası oyuncunun üst kısmına ne kadar yakın olmalı (0.18 => üst %18).")]
    [Range(0.01f, 0.5f)]
    public float ledgeHopTopZone = 0.18f;

    [Tooltip("Yan temas saymak için normal.x eşiği")]
    [Range(0.1f, 1f)]
    public float ledgeHopSideNormalMin = 0.6f;

    [Tooltip("Yukarı/aşağı normal çok büyükse (zemin gibi) sayma")]
    [Range(0f, 1f)]
    public float ledgeHopMaxAbsNormalY = 0.35f;

    [Tooltip("Spam'i önlemek için minimum süre (sn).")]
    public float ledgeHopCooldown = 0.08f;

    [Header("Head Edge Correction (corner bump assist)")]
    [Tooltip("0 yaparsan kapalı. Kafayla köşe vurunca uygulanacak yatay düzeltme (world unit).")]
    public float headEdgeCorrectionDistance = 0.06f;

    [Tooltip("Spam'i önler (sn).")]
    public float headEdgeCorrectionCooldown = 0.08f;

    [Tooltip("Köşe düzeltmesi sırasında vy bunun altına düşmesin (0.5–2 iyi).")]
    public float headEdgeMinUpVelocity = 1.2f;

    [Tooltip("Temas noktası üst bölgeye ne kadar yakın olmalı (0.2 => üst %20).")]
    [Range(0.01f, 0.5f)]
    public float headEdgeTopZone = 0.20f;

    [Tooltip("Temas noktası yan kenara ne kadar yakın olmalı (0.12 => geniş tolerans). world unit.")]
    public float headEdgeSideInset = 0.12f;

    [Tooltip("Ceiling saymak için normal.y eşiği (daha negatif = daha kesin tavan).")]
    public float headEdgeCeilingNormalYMax = -0.6f;

    [SerializeField] private StandLock2D standLock;
    public bool IsCrouching { get; private set; }

    private float headEdgeCdTimer;
    private Vector2 queuedPositionNudge;


    // Internal
    private Rigidbody2D rb;
    private Collider2D bodyCol;

    private bool isGrounded;
    private float coyoteTimer;
    private float bufferTimer;

    private float ledgeHopCdTimer;

    // Public API (diğer scriptler için)
    public bool IsGrounded => isGrounded;
    public Rigidbody2D Rb => rb;
    public bool IsFacingRight => isFacingRight;

    private void Awake()
    {
        i = this;

        if (standLock == null) standLock = GetComponent<StandLock2D>();

        rb = GetComponent<Rigidbody2D>();
        bodyCol = GetComponent<Collider2D>();

        rb.freezeRotation = true;

        // Gravity'yi biz yöneteceğiz
        rb.gravityScale = 0f;
        input = InputController2D.Current;
        input = inputBehaviour as IPlayerInput2D;
        if (input == null) input = InputController2D.Current;

        if (graphics != null)
        {
            graphicsBaseScale = graphics.localScale;
            isFacingRight = graphicsBaseScale.x >= 0f;
        }
        else
        {
            graphicsBaseScale = Vector3.one;
            isFacingRight = true;
        }
    }

    private void Update()
    {
        if (input == null) return;
        Debug.Log("Control held" +input.ControlHeld);
        Debug.Log("Control pres" + input.ControlPressed);

        Debug.Log("dash held" + input.ShiftHeld);
        Debug.Log("dash press" +input.ShiftPressed);

        // Ground
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        // Timers
        coyoteTimer = isGrounded ? coyoteTime : coyoteTimer - Time.deltaTime;

        if (input.JumpPressed) bufferTimer = jumpBuffer;
        else bufferTimer -= Time.deltaTime;

        // Facing flip
        UpdateFacingFromInput(input.Move.x);

        if (isGrounded)
        {
            isSprinting = input.ShiftHeld;
        }
        if (isGrounded && !input.ShiftHeld)
            isSprinting = false;

        if (isGrounded)
        {
            isCrouching = input.ControlHeld;
        }
        else
        {
            isCrouching = false;
        }
        if (isGrounded && !input.ControlHeld)
            isCrouching = false;

        bool wantsCrouch = (input != null) && input.ControlHeld;
        bool locked = (standLock != null) && standLock.IsLocked;

        // Ctrl bırakılsa bile üstte engel varsa crouch devam
        IsCrouching = wantsCrouch || locked;
    }

    private void FixedUpdate()
    {
        if (input == null) return;

        if (headEdgeCdTimer > 0f)
            headEdgeCdTimer -= Time.fixedDeltaTime;

        // queue edilmiş nudge varsa uygula (çok küçük bir düzeltme)
        if (queuedPositionNudge != Vector2.zero)
        {
            rb.position += queuedPositionNudge;
            queuedPositionNudge = Vector2.zero;
        }


        if (ledgeHopCdTimer > 0f)
            ledgeHopCdTimer -= Time.fixedDeltaTime;

        Vector2 v = rb.linearVelocity;

        // ---------------- Horizontal ----------------
        float targetX;
        if (isSprinting)
        {
            targetX = (float)(input.Move.x * maxRunSpeed);
        }
        else if (isCrouching)
        {
            targetX = (float)(input.Move.x * maxCrouchSpeed);
        }
        else
        {
            targetX = (float)(input.Move.x * maxWalkSpeed);
        }

        float accel = isGrounded
            ? (Mathf.Abs(targetX) > 0.01f ? groundAccel : groundDecel)
            : (Mathf.Abs(targetX) > 0.01f ? airAccel : airDecel);

        v.x = Mathf.MoveTowards(v.x, targetX, accel * Time.fixedDeltaTime);

        // ---------------- Jump trigger ----------------
        if (bufferTimer > 0f && coyoteTimer > 0f)
        {
            v.y = jumpVelocity;   // snappy takeoff
            bufferTimer = 0f;
            coyoteTimer = 0f;
            isGrounded = false;
        }

        // ---------------- Custom gravity ----------------
        float g = (v.y > 0f) ? gravityUp : gravityDown;

        // Apex civarında “asılı kalmasın”
        if (Mathf.Abs(v.y) < apexBand)
            g += apexGravityBonus;

        // Variable height
        if (!input.JumpHeld && v.y > 0f)
            g += jumpCutGravityBonus;

        v.y -= g * Time.fixedDeltaTime;

        // Terminal fall speed
        if (v.y < -maxFallSpeed) v.y = -maxFallSpeed;

        rb.linearVelocity = v;
    }

    // ========== Ledge Hop: only while airborne + corner contact ==========
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryLedgeHop(collision);          
        TryHeadEdgeCorrection(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryLedgeHop(collision);          
        TryHeadEdgeCorrection(collision);
    }

    private void TryHeadEdgeCorrection(Collision2D col)
    {
        if (headEdgeCorrectionDistance <= 0f) return;
        if (isGrounded) return;                    // yerdeyken değil
        if (headEdgeCdTimer > 0f) return;
        if (bodyCol == null) return;

        // Sadece zıplarken / yukarı giderken
        if (rb.linearVelocity.y <= 0.01f) return;

        // Sadece groundLayer ile temas (tavan da ground layer'daysa)
        int otherLayerMask = 1 << col.collider.gameObject.layer;
        if ((groundLayer.value & otherLayerMask) == 0) return;

        // bounds
        Bounds b = bodyCol.bounds;
        float topY = b.max.y;
        float topZoneY = topY - b.size.y * headEdgeTopZone;

        float leftX = b.min.x;
        float rightX = b.max.x;
        float leftEdgeX = leftX + headEdgeSideInset;
        float rightEdgeX = rightX - headEdgeSideInset;

        foreach (var c in col.contacts)
        {
            // Tavan teması: normal aşağı doğru
            if (c.normal.y > headEdgeCeilingNormalYMax) continue; // örn -0.6'dan daha aşağı olmalı

            // Temas üst bölgede mi?
            if (c.point.y < topZoneY) continue;

            // Köşe hissi: temas X'i yan kenara yakın olmalı
            bool nearLeftEdge = c.point.x <= leftEdgeX;
            bool nearRightEdge = c.point.x >= rightEdgeX;
            if (!nearLeftEdge && !nearRightEdge) continue;

            // Hangi yöne itelim?
            // Temas soldaysa sağa it; sağdaysa sola it.
            float pushDir = nearLeftEdge ? 1f : -1f;

            // Çok küçük yatay nudge queue et (bir sonraki FixedUpdate'te uygularız)
            queuedPositionNudge = new Vector2(pushDir * headEdgeCorrectionDistance, 0f);

            // Jump "ölmesin": vy aniden kesilmesin
            Vector2 v = rb.linearVelocity;
            if (v.y < headEdgeMinUpVelocity)
                v.y = headEdgeMinUpVelocity;
            rb.linearVelocity = v;

            headEdgeCdTimer = headEdgeCorrectionCooldown;
            return;
        }
    }

    private void TryLedgeHop(Collision2D col)
    {
        if (ledgeHopVelocity <= 0f) return;
        if (isGrounded) return;
        if (ledgeHopCdTimer > 0f) return;
        if (bodyCol == null) return;

        // Sadece groundLayer ile temas
        int otherLayerMask = 1 << col.collider.gameObject.layer;
        if ((groundLayer.value & otherLayerMask) == 0) return;

        // Yükselirken OK, düşerken sadece düşüşün başında OK
        if (rb.linearVelocity.y < -ledgeHopAllowedDownSpeed) return;

        float topY = bodyCol.bounds.max.y;
        float zoneY = topY - bodyCol.bounds.size.y * ledgeHopTopZone - ledgeHopExtraTopOffset;

        foreach (var c in col.contacts)
        {
            if (Mathf.Abs(c.normal.x) < ledgeHopSideNormalMin) continue;
            if (Mathf.Abs(c.normal.y) > ledgeHopMaxAbsNormalY) continue;
            if (c.point.y < zoneY) continue;

            Vector2 v = rb.linearVelocity;
            if (v.y < ledgeHopVelocity)
            {
                v.y = ledgeHopVelocity;
                rb.linearVelocity = v;
            }

            ledgeHopCdTimer = ledgeHopCooldown;
            return;
        }
    }

    // ========== Facing ==========
    private void UpdateFacingFromInput(float moveX)
    {
        if (Mathf.Abs(moveX) < faceDeadzone) return;

        if (moveX > 0f) SetFacing(true);
        else if (moveX < 0f) SetFacing(false);
    }


    private void SetFacing(bool faceRight)
    {
        if (isFacingRight == faceRight) return;
        isFacingRight = faceRight;

        if (graphics == null) return;

        Vector3 s = graphicsBaseScale;
        s.x = Mathf.Abs(s.x) * (isFacingRight ? 1f : -1f);
        graphics.localScale = s;
    }


    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
