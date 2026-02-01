using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class UILevelLayerController : MonoBehaviour
{
    [Header("UI (aynı sırada: 0..n-1)")]
    [SerializeField] private Button[] buttons;

    [Header("Layers (aynı sırada: 0..n-1)")]
    [SerializeField] private GameObject[] layers;

    [Header("Block rule")]
    [Tooltip("Player'ın Collider2D'sini buraya ver. Player bu tilemap ile çakışıyorsa click ile açma engellenecek.")]
    [SerializeField] private Collider2D playerCollider;

    // click ile kalıcı açık mı?
    private bool[] toggledOn;

    // hover ile preview açıldı mı?
    private bool[] previewOpened;

    void Awake()
    {
        int n = Mathf.Min(buttons?.Length ?? 0, layers?.Length ?? 0);
        if (n <= 0)
        {
            Debug.LogError("[UILevelLayerController] Buttons/Layers boş veya eşleşmiyor.");
            return;
        }

        if (playerCollider == null)
            playerCollider = PlayerMovement2D.i.gameObject.GetComponentInChildren<Collider2D>();

        toggledOn = new bool[n];
        previewOpened = new bool[n];

        for (int i = 0; i < n; i++)
        {
            toggledOn[i] = false;
            previewOpened[i] = false;

            if (layers[i] != null)
                layers[i].SetActive(false);
        }

        for (int i = 0; i < n; i++)
        {
            int index = i;

            if (buttons[index] == null) continue;

            var handler = buttons[index].gameObject.GetComponent<HoverHandler>();
            if (handler == null) handler = buttons[index].gameObject.AddComponent<HoverHandler>();
            handler.Init(
                onEnter: () => HoverLayer(index),
                onExit: () => ExitLayer(index)
            );

            buttons[index].onClick.AddListener(() => ManageLayer(index));
        }
    }

    // =========================
    // CLICK: kalıcı aç/kapat
    // =========================
    public void ManageLayer(int index)
    {
        if (!IsValid(index)) return;

        Tilemap tm = GetTilemap(index);
        if (tm == null) return;

        bool newState = !toggledOn[index];

        if (newState)
        {
            // ✅ KURAL: Player bu tilemap'in dolu tile alanıyla çakışıyorsa AÇMA
            if (playerCollider != null && IsPlayerOverlappingTilemap(tm, playerCollider))
            {
                // toggle geri
                toggledOn[index] = false;
                previewOpened[index] = false;

                // İstersen burada uyarı sesi/animasyonu tetiklersin
                // Debug.Log("Blocked: Player is overlapping tilemap. Can't open.");
                return;
            }

            toggledOn[index] = true;
            previewOpened[index] = false;
            RestoreToNormal(tm); // aktif + collider açık + alpha 1
        }
        else
        {
            toggledOn[index] = false;
            previewOpened[index] = false;
            tm.gameObject.SetActive(false);
        }
    }

    // =========================
    // HOVER: sadece preview aç
    // =========================
    public void HoverLayer(int index)
    {
        if (!IsValid(index)) return;
        if (toggledOn[index]) return;
        if (previewOpened[index]) return;

        Tilemap tm = GetTilemap(index);
        if (tm == null) return;

        previewOpened[index] = true;
        ActivatePreview(tm);
    }

    // =========================
    // EXIT: sadece preview kapat
    // =========================
    public void ExitLayer(int index)
    {
        if (!IsValid(index)) return;
        if (toggledOn[index]) return;
        if (!previewOpened[index]) return;

        Tilemap tm = GetTilemap(index);
        if (tm == null) return;

        previewOpened[index] = false;
        RestorePreviewAndDisable(tm);
    }

    // =========================
    // Tilemap yardımcıları
    // =========================
    private Tilemap GetTilemap(int index)
    {
        if (layers[index] == null) return null;
        return layers[index].GetComponent<Tilemap>();
    }

    private bool IsValid(int index)
    {
        return toggledOn != null && index >= 0 && index < toggledOn.Length;
    }

    // =========================================================
    // ✅ PLAYER - TILEMAP overlap kontrolü (tile data üzerinden)
    // =========================================================
    private static bool IsPlayerOverlappingTilemap(Tilemap tilemap, Collider2D playerCol)
    {
        if (tilemap == null || playerCol == null) return false;

        Bounds pb = playerCol.bounds;

        // Player bounds'un kapsadığı cell aralığı
        Vector3Int min = tilemap.WorldToCell(pb.min);
        Vector3Int max = tilemap.WorldToCell(pb.max);

        // Tilemap cell size (world)
        Vector3 cellSize = tilemap.cellSize;
        Vector2 half = new Vector2(cellSize.x * 0.5f, cellSize.y * 0.5f);

        // Küçük güvenlik: z çok önemli değil, 2D’de x-y bakıyoruz
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                if (!tilemap.HasTile(cell))
                    continue;

                // cell'in world rect'i (tile anchor/offset farklıysa yine de iyi çalışır)
                Vector3 center3 = tilemap.GetCellCenterWorld(cell);
                Vector2 center = new Vector2(center3.x, center3.y);

                // Tile rect bounds
                float left = center.x - half.x;
                float right = center.x + half.x;
                float bottom = center.y - half.y;
                float top = center.y + half.y;

                // Player bounds ile kesişim kontrolü
                if (pb.max.x > left && pb.min.x < right && pb.max.y > bottom && pb.min.y < top)
                    return true;
            }
        }

        return false;
    }

    // -------------------------
    // PREVIEW ON (hover)
    // -------------------------
    private static void ActivatePreview(Tilemap tilemap)
    {
        if (tilemap == null) return;

        tilemap.gameObject.SetActive(true);

        var collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null) collider.enabled = false;

        var composite = tilemap.GetComponent<CompositeCollider2D>();
        if (composite != null) composite.enabled = false;

        Color c = tilemap.color;
        c.a = 0.5f;
        tilemap.color = c;
    }

    // -------------------------
    // NORMAL ON (click aç)
    // -------------------------
    private static void RestoreToNormal(Tilemap tilemap)
    {
        if (tilemap == null) return;

        tilemap.gameObject.SetActive(true);

        var collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null) collider.enabled = true;

        var composite = tilemap.GetComponent<CompositeCollider2D>();
        if (composite != null) composite.enabled = true;

        Color c = tilemap.color;
        c.a = 1f;
        tilemap.color = c;
    }

    // -------------------------
    // PREVIEW OFF (exit)
    // -------------------------
    private static void RestorePreviewAndDisable(Tilemap tilemap)
    {
        if (tilemap == null) return;

        var collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null) collider.enabled = true;

        var composite = tilemap.GetComponent<CompositeCollider2D>();
        if (composite != null) composite.enabled = true;

        Color c = tilemap.color;
        c.a = 1f;
        tilemap.color = c;

        tilemap.gameObject.SetActive(false);
    }

    // =========================
    // HOVER HANDLER (tek dosya)
    // =========================
    private class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private System.Action enter;
        private System.Action exit;

        public void Init(System.Action onEnter, System.Action onExit)
        {
            enter = onEnter;
            exit = onExit;
        }

        public void OnPointerEnter(PointerEventData eventData) => enter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => exit?.Invoke();
    }
}
