using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationSetter : MonoBehaviour
{
    [SerializeField] private PlayerAnimController controller;
    [SerializeField] private float fadeTime = 0.05f;

    private Animator anim;
    private int currentHash;

    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int JumpUp = Animator.StringToHash("JumpUp");
    private static readonly int Apex = Animator.StringToHash("Apex");
    private static readonly int Fall = Animator.StringToHash("Fall");
    private static readonly int Land = Animator.StringToHash("Land");
    private static readonly int Dash = Animator.StringToHash("Dash");
    private static readonly int CrouchWalk = Animator.StringToHash("CrouchWalk");
    private static readonly int CrouchIdle = Animator.StringToHash("CrouchIdle");
    private static readonly int Roll = Animator.StringToHash("Roll");




    private void Awake()
    {
        anim = GetComponent<Animator>();
        currentHash = 0;
    }

    private void Update()
    {
        if (controller == null) return;

        var info = anim.GetCurrentAnimatorStateInfo(0);


        int target = controller.State switch
        {
            PlayerAnimState.Idle => Idle,
            PlayerAnimState.Walk => Walk,
            PlayerAnimState.JumpUp => JumpUp,
            PlayerAnimState.Apex => Apex,
            PlayerAnimState.Fall => Fall,
            PlayerAnimState.Land => Land,
            PlayerAnimState.Run => Run,
            PlayerAnimState.Dash => Dash,
            PlayerAnimState.CrouchWalk=> CrouchWalk,
            PlayerAnimState.CrouchIdle => CrouchIdle,
            PlayerAnimState.Roll => Roll,
            _ => Idle
        };

        if (target == currentHash) return;

        anim.CrossFadeInFixedTime(target, fadeTime, 0);
        currentHash = target;
        if (!anim.HasState(0, target))
            Debug.LogError("Animator state bulunamadý: " + controller.State);

    }
}
