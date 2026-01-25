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

    // ✅ Yeni: Dash isteği (one-shot)
    public bool DashRequested { get; private set; }

    public bool ControlPressed { get; private set; }

    public bool ControlHeld { get; private set; }

    public bool SlideRequested { get; private set; }


    [Header("Dash Tap Settings")]
    [Tooltip("Shift bu süreden kısa basılı kalıp bırakılırsa DashRequested=true olur. 0 => kapalı.")]
    [SerializeField] private float dashTapWindow = 0.18f;

    [Header("Ctrl Tap Settings")]
    [Tooltip("Ctrl bu süreden kısa basılı kalıp bırakılırsa Slide=true olur. 0 => kapalı.")]
    [SerializeField] private float slideTapWindow = 0.18f;

    private float shiftDownTime;
    private float slideDownTime;


    private void Awake()
    {
        controls = new Controls();
        Current = this; // singleton
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

        // Shift DOWN
        controls.PCGamePlay.LShift.performed += ctx =>
        {
            ShiftPressed = true;
            ShiftHeld = true;

            shiftDownTime = Time.time;
            if (PlayerMovement2D.i.IsGrounded == false)
            {
                DashRequested = true;
            }
        };

        // Shift UP
        controls.PCGamePlay.LShift.canceled += ctx =>
        {
            ShiftHeld = false;

            // kısa bas-bırak => dash isteği üret
            if (dashTapWindow > 0f&& PlayerMovement2D.i.IsGrounded)
            {
                float held = Time.time - shiftDownTime;
                if (held <= dashTapWindow)
                    DashRequested = true;
            }
        };

        // Ctrl DOWN
        controls.PCGamePlay.LCtrl.performed += ctx =>
        {
            ControlPressed = true;
            ControlHeld = true;

            slideDownTime = Time.time;
        };

        // Ctrl UP
        controls.PCGamePlay.LCtrl.canceled += ctx =>
        {
            ControlHeld = false;

            // kısa bas-bırak => slide isteği üret
            if (slideTapWindow > 0f && PlayerMovement2D.i.IsGrounded)
            {
                float held = Time.time - slideDownTime;
                if (held <= slideTapWindow)
                    SlideRequested = true;
            }
        };
    }

    private void OnDisable()
    {
        controls.Disable();
        if (ReferenceEquals(Current, this)) Current = null;
    }

    private void LateUpdate()
    {
        // one-shot flag'ler burada sıfırlanır (diğer scriptler Update'te okur)
        JumpPressed = false;
        ShiftPressed = false;
        DashRequested = false;
        ControlPressed = false;
        SlideRequested = false;
    }
}
