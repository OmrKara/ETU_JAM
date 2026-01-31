using UnityEngine;

public class StandLock2D : MonoBehaviour
{
    [Header("Headroom Check")]
    [SerializeField] private Transform headCheck;
    [SerializeField] private Vector2 headCheckSize = new Vector2(0.7f, 0.25f);
    [SerializeField] private LayerMask solidLayer;

    [Header("Smoothing (optional)")]
    [Tooltip("Engel kaybolduktan sonra ayaða kalkmayý bu kadar geciktir (sn). 0 => anýnda.")]
    [SerializeField] private float unlockDelay = 0.0f;

    public bool IsLocked { get; private set; }   // Üstte engel var => ayakta duramaz
    public bool CanStand => !IsLocked;

    private float unlockTimer;

    private void Update()
    {
        bool blocked = IsHeadBlocked();

        if (blocked)
        {
            IsLocked = true;
            unlockTimer = unlockDelay;
        }
        else
        {
            if (unlockDelay <= 0f)
            {
                IsLocked = false;
            }
            else
            {
                if (unlockTimer > 0f)
                {
                    unlockTimer -= Time.deltaTime;
                    IsLocked = true;
                }
                else
                {
                    IsLocked = false;
                }
            }
        }
    }

    private bool IsHeadBlocked()
    {
        if (headCheck == null) return false;
        return Physics2D.OverlapBox(headCheck.position, headCheckSize, 0f, solidLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (headCheck == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(headCheck.position, headCheckSize);
    }
}
