using System.Collections;
using UnityEngine;
using static GameHandler;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemType itemType;

    [Header("UI")]
    [SerializeField] private GameObject uiImage; // Canvas'taki Image

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string collectTrigger = "Collect";
    [SerializeField] private float destroyDelay = 0.4f;

    private bool collected;
    private Collider2D col;

    private void Awake()
    {
        switch (itemType)
        {
            case ItemType.Turkuaz:
                uiImage = FindFirstObjectByType<TurkuazImage>().gameObject;
                break;
            case ItemType.GreenMaskShard1:
                uiImage = FindFirstObjectByType<GreenShard1>().gameObject;
                break;
            case ItemType.GreenMaskShard2:
                uiImage = FindFirstObjectByType<GreenShard2>().gameObject;
                break;
            default:
                break;
        }
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        Collect();
    }

    private void Collect()
    {
        collected = true;

        // inventory
        GameHandler.I.AddItem(itemType);

        // tekrar trigger olmasýn
        col.enabled = false;

        // UI Image aç
        if (uiImage != null)
            uiImage.SetActive(true);

        // animasyon
        if (animator != null)
            animator.Play(collectTrigger);

        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
