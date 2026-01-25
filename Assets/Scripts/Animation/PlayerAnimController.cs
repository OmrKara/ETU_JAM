using UnityEngine;

public enum PlayerAnimState
{
    Idle,
    Walk,
    Run,
    Dash,
    JumpUp,
    Apex,   // TEK geçiş animasyonu
    Fall,
    Land
}

public class PlayerAnimController : MonoBehaviour
{
    private PlayerMovement2D movement;

    [Header("Thresholds")]
    [SerializeField] private float runVelDeadzone = 0.05f;
    [SerializeField] private float vyUp = 0.10f;      // vy > 0.10 => JumpUp
    [SerializeField] private float vyDown = -0.10f;   // vy < -0.10 => Fall
    [SerializeField] private float apexBand = 0.10f;  // |vy| <= band => tepe bölgesi

    [Header("Min show times (set 0 to disable)")]
    [Tooltip("0 yaparsan Apex hiç görünmez.")]
    [SerializeField] private float apexMinTime = 0.06f;
    [SerializeField] private float landMinTime = 0.08f;

    public PlayerAnimState State { get; private set; } = PlayerAnimState.Idle;

    public bool Grounded { get; private set; }
    public float Vy { get; private set; }
    public float VxAbs { get; private set; }

    private bool wasGrounded;
    private float prevVy;

    private float apexTimer;
    private float landTimer;

    private void Start()
    {
        movement = PlayerMovement2D.i;
        wasGrounded = true;
        prevVy = 0f;
    }

    private void Update()
    {

        if (movement == null || movement.Rb == null)
        {
            State = PlayerAnimState.Idle;
            return;
        }

        Grounded = movement.IsGrounded;
        Vy = movement.Rb.linearVelocity.y;
        VxAbs = Mathf.Abs(movement.Rb.linearVelocity.x);

        bool isAir = !Grounded;
        if (PlayerDash2D.Instance.IsDashing)
        {
            State = PlayerAnimState.Dash;
            return;
        }
        // Landing detect
        if (!wasGrounded && Grounded && landMinTime > 0f)
            landTimer = landMinTime;

        // Apex tetikleme: yukarı gidiyorduk, tepe bandına girdik (vy pozitiften |vy|<=band)
        // Böylece Apex sadece "yukarı->aşağı geçiş anında" devreye girer.
        if (isAir && apexMinTime > 0f)
        {
            bool enteredApexZoneFromUp = (prevVy > apexBand) && (Mathf.Abs(Vy) <= apexBand);
            if (enteredApexZoneFromUp)
                apexTimer = apexMinTime;
        }

        prevVy = Vy;
        wasGrounded = Grounded;

        // Öncelik: Land > ApexTimer > normal
        if (landTimer > 0f)
        {
            landTimer -= Time.deltaTime;
            State = PlayerAnimState.Land;
            return;
        }

        if (isAir)
        {
            if (apexTimer > 0f)
            {
                apexTimer -= Time.deltaTime;
                State = PlayerAnimState.Apex;
                return;
            }

            if (Vy > vyUp) State = PlayerAnimState.JumpUp;
            else if (Vy < vyDown) State = PlayerAnimState.Fall;
            else State = PlayerAnimState.Fall; // band içindeyken timer yoksa direkt Fall'a kay (tercih)
            return;
        }
        if ((VxAbs > runVelDeadzone))
        {
            if (InputController2D.Current.ShiftHeld)
            {
                State = PlayerAnimState.Run;
            }
            else
            {
                State = PlayerAnimState.Walk;
            }
        }
        else
        {
            State = PlayerAnimState.Idle;
        }


    }
}
