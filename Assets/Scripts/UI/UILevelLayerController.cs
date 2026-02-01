using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class UILevelLayerController : MonoBehaviour
{
    [Header("UI Buttons (boş bırakılabilir)")]
    [SerializeField] private Button[] buttons;

    [Header("Tilemap Layers (boş bırakılabilir)")]
    [SerializeField] private GameObject[] layers;

    [Header("Player")]
    [SerializeField] private Collider2D playerCollider;

    // State (index bazlı)
    private bool[] toggledOn;
    private bool[] previewOpened;

    private void Start()
    {
        playerCollider = PlayerMovement2D.i.collider;
    }
    void Awake()
    {
        int n = Mathf.Max(buttons?.Length ?? 0, layers?.Length ?? 0);
        if (n == 0)
        {
            Debug.LogError("[UILevelLayerController] Buttons/Layers array boş.");
            return;
        }

        toggledOn = new bool[n];
        previewOpened = new bool[n];

        // Başlangıçta var olan layer'ları kapat
        for (int i = 0; i < n; i++)
        {
            toggledOn[i] = false;
            previewOpened[i] = false;

            GameObject layerObj = GetLayerObj(i);
            if (layerObj != null)
                layerObj.SetActive(false);
        }

        // Sadece button+layer ikisi de varsa bağla
        for (int i = 0; i < n; i++)
        {
            Button btn = GetButton(i);
            GameObject layerObj = GetLayerObj(i);

            if (btn == null || layerObj == null)
                continue; // ✅ boş slot: atla

            int index = i;

            // Hover/Exit handler (varsa yeniden ekleme)
            HoverHandler h = btn.GetComponent<HoverHandler>();
            if (h == null) h = btn.gameObject.AddComponent<HoverHandler>();
            h.Init(
                () => HoverLayer(index),
                () => ExitLayer(index)
            );

            // Click: çift listener olmasın diye önce temizle
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ManageLayer(index));
        }
    }

    // =========================
    // CLICK (kalıcı aç/kapat)
    // =========================
    public void ManageLayer(int index)
    {
        if (!HasValidPair(index)) return;

        Tilemap tm = GetTilemap(index);
        if (tm == null) return;

        bool wantOpen = !toggledOn[index];

        if (wantOpen)
        {
            // ❌ Player tilemap ile çakışıyorsa AÇMA
            // Tilemap boşsa zaten false döner ve bloklamaz.
            if (playerCollider != null && IsPlayerOverlappingTilemap(tm, playerCollider))
            {
                // ⚠️ previewOpened'a dokunma → exit düzgün kapatsın
                return;
            }

            toggledOn[index] = true;
            previewOpened[index] = false;
            RestoreToNormal(tm);
        }
        else
        {
            toggledOn[index] = false;
            previewOpened[index] = false;
            tm.gameObject.SetActive(false);
        }
    }

    // =========================
    // HOVER (preview aç)
    // =========================
    public void HoverLayer(int index)
    {
        if (!HasValidPair(index)) return;
        if (toggledOn[index]) return;
        if (previewOpened[index]) return;

        Tilemap tm = GetTilemap(index);
        if (tm == null) return;

        previewOpened[index] = true;
        ActivatePreview(tm);
    }

    // =========================
    // EXIT (preview kapat)
    // =========================
    public void ExitLayer(int index)
    {
        if (!HasValidPair(index)) return;
        if (toggledOn[index]) return;
        if (!previewOpened[index]) return;

        Tilemap tm = GetTilemap(index);
        if (tm == null) return;

        previewOpened[index] = false;
        RestorePreviewAndDisable(tm);
    }

    // =========================
    // Helpers (null-safe)
    // =========================
    private Button GetButton(int index)
    {
        if (buttons == null) return null;
        if (index < 0 || index >= buttons.Length) return null;
        return buttons[index];
    }

    private GameObject GetLayerObj(int index)
    {
        if (layers == null) return null;
        if (index < 0 || index >= layers.Length) return null;
        return layers[index];
    }

    private bool HasValidPair(int index)
    {
        return GetButton(index) != null && GetLayerObj(index) != null;
    }

    private Tilemap GetTilemap(int index)
    {
        GameObject obj = GetLayerObj(index);
        if (obj == null) return null;
        return obj.GetComponent<Tilemap>();
    }

    // =========================
    // Tilemap Modları
    // =========================
    private static void ActivatePreview(Tilemap tilemap)
    {
        tilemap.gameObject.SetActive(true);

        var col = tilemap.GetComponent<TilemapCollider2D>();
        if (col) col.enabled = false;

        var comp = tilemap.GetComponent<CompositeCollider2D>();
        if (comp) comp.enabled = false;

        Color c = tilemap.color;
        c.a = 0.5f;
        tilemap.color = c;
    }

    private static void RestoreToNormal(Tilemap tilemap)
    {
        tilemap.gameObject.SetActive(true);

        var col = tilemap.GetComponent<TilemapCollider2D>();
        if (col) col.enabled = true;

        var comp = tilemap.GetComponent<CompositeCollider2D>();
        if (comp) comp.enabled = true;

        Color c = tilemap.color;
        c.a = 1f;
        tilemap.color = c;
    }

    private static void RestorePreviewAndDisable(Tilemap tilemap)
    {
        var col = tilemap.GetComponent<TilemapCollider2D>();
        if (col) col.enabled = true;

        var comp = tilemap.GetComponent<CompositeCollider2D>();
        if (comp) comp.enabled = true;

        Color c = tilemap.color;
        c.a = 1f;
        tilemap.color = c;

        tilemap.gameObject.SetActive(false);
    }

    // =========================
    // Player - Tilemap overlap
    // (Tilemap boşsa false)
    // =========================
    private static bool IsPlayerOverlappingTilemap(Tilemap tilemap, Collider2D player)
    {
        Bounds pb = player.bounds;

        Vector3Int min = tilemap.WorldToCell(pb.min);
        Vector3Int max = tilemap.WorldToCell(pb.max);

        Vector3 cellSize = tilemap.cellSize;
        Vector2 half = cellSize * 0.5f;

        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (!tilemap.HasTile(cell)) continue;

                Vector3 center = tilemap.GetCellCenterWorld(cell);

                if (pb.max.x > center.x - half.x &&
                    pb.min.x < center.x + half.x &&
                    pb.max.y > center.y - half.y &&
                    pb.min.y < center.y + half.y)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // =========================
    // Hover Handler (tek dosya)
    // =========================
    private class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private System.Action onEnter;
        private System.Action onExit;

        public void Init(System.Action enter, System.Action exit)
        {
            onEnter = enter;
            onExit = exit;
        }

        public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke();
    }
}
