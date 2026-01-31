using UnityEngine;
using UnityEngine.InputSystem;

public class InputController2D : MonoBehaviour, IPlayerInput2D
{
    public static IPlayerInput2D Current { get; private set; }

    private Controls controls;

    public Vector2 Move { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }

    public bool ShiftPressed { get; private set; }
    public bool ShiftHeld { get; private set; }

    public bool DashRequested { get; private set; }   // one-shot

    public bool ControlPressed { get; private set; }
    public bool ControlHeld { get; private set; }     // ✅ gecikmeli
    public bool SlideRequested { get; private set; }  // one-shot

    [Header("Dash Tap Settings")]
    [Tooltip("Shift bu süreden kısa basılı kalıp bırakılırsa DashRequested=true olur. 0 => kapalı.")]
    [SerializeField] private float dashTapWindow = 0.18f;

    [Header("Ctrl Tap Settings")]
    [Tooltip("Ctrl bu süreden kısa basılı kalıp bırakılırsa SlideRequested=true olur. 0 => kapalı.")]
    [SerializeField] private float slideTapWindow = 0.18f;

    private float shiftDownTime;

    // ✅ Ctrl için “deferred hold” state
    private float ctrlDownTime;
    private bool ctrlIsDown;
    private bool ctrlHoldActivated; // tapWindow geçti mi?

    private void Awake()
    {
        controls = new Controls();
        Current = this;
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.PCGamePlay.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        controls.PCGamePlay.Move.canceled += ctx => Move = Vector2.zero;

        controls.PCGamePlay.Jump.performed += ctx =>
        {
            JumpPressed = true;
            JumpHeld = true;
        };
        controls.PCGamePlay.Jump.canceled += ctx => JumpHeld = false;

        // ===== Shift (Dash tap) =====
        controls.PCGamePlay.LShift.performed += ctx =>
        {
            ShiftPressed = true;
            ShiftHeld = true;
            shiftDownTime = Time.time;

            // ✅ AIR DASH: basıldığı anda
            if (PlayerMovement2D.i != null && !PlayerMovement2D.i.IsGrounded)
            {
                DashRequested = true;
            }
        };

        controls.PCGamePlay.LShift.canceled += ctx =>
        {
            ShiftHeld = false;

            // ✅ GROUND DASH: tap (bas-bırak)
            if (dashTapWindow > 0f && PlayerMovement2D.i != null && PlayerMovement2D.i.IsGrounded)
            {
                float held = Time.time - shiftDownTime;
                if (held <= dashTapWindow)
                    DashRequested = true;
            }
        };

        // ===== Ctrl (Slide tap, Crouch hold after window) =====
        controls.PCGamePlay.LCtrl.performed += ctx =>
        {
            ControlPressed = true;

            ctrlIsDown = true;
            ctrlDownTime = Time.time;
            ctrlHoldActivated = false;

            // ⚠️ burada ControlHeld TRUE YAPMIYORUZ
            // crouch’a geçmek için tapWindow’un bitmesini bekleyeceğiz
        };

        controls.PCGamePlay.LCtrl.canceled += ctx =>
        {
            ctrlIsDown = false;

            // TapWindow dolmadan bırakıldıysa => SLIDE
            if (!ctrlHoldActivated && slideTapWindow > 0f && PlayerMovement2D.i != null && PlayerMovement2D.i.IsGrounded)
            {
                float held = Time.time - ctrlDownTime;
                if (held <= slideTapWindow)
                    SlideRequested = true;
            }

            // bırakınca crouch biter
            ControlHeld = false;
            ctrlHoldActivated = false;
        };
    }

    private void Update()
    {
        // Ctrl basılı kaldı ve tapWindow geçtiyse artık crouch aktif
        if (ctrlIsDown && !ctrlHoldActivated && slideTapWindow > 0f)
        {
            if (Time.time - ctrlDownTime >= slideTapWindow)
            {
                ctrlHoldActivated = true;
                ControlHeld = true; // ✅ artık crouch state tetiklenebilir
            }
        }

        // slideTapWindow 0 yapılırsa: klasik davranış (hemen crouch)
        if (ctrlIsDown && slideTapWindow <= 0f)
            ControlHeld = true;
    }

    private void OnDisable()
    {
        controls.Disable();
        if (ReferenceEquals(Current, this)) Current = null;
    }

    private void LateUpdate()
    {
        // one-shot flag’leri sıfırla
        JumpPressed = false;
        ShiftPressed = false;
        DashRequested = false;

        ControlPressed = false;
        SlideRequested = false;
    }
}
