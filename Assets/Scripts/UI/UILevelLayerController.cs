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

    [Header("Player (kapalıyken açmayı engellemek için)")]
    [SerializeField] private Collider2D playerCollider;

    // Kalıcı açık mı?
    private bool[] toggledOn;

    // Tilemap kapalıyken hover preview açıldı mı?
    private bool[] previewOpened;

    // Tilemap AÇIKKEN hover olunca sadece alpha düşürme aktif mi?
    private bool[] dimmedWhileOpen;

    // Pointer şu an hangi index üstünde? (hover ile güncellenir)
    private int pointerOverIndex = -1;

    private void Update() { playerCollider = PlayerMovement2D.i.collider; }
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
        dimmedWhileOpen = new bool[n];

        // Başlangıçta layer'ları kapat
        for (int i = 0; i < n; i++)
        {
            toggledOn[i] = false;
            previewOpened[i] = false;
            dimmedWhileOpen[i] = false;

            GameObject layerObj = GetLayerObj(i);
            if (layerObj != null)
                layerObj.SetActive(false);
        }

        // Bağla (sadece button + layer ikisi de varsa)
        for (int i = 0; i < n; i++)
        {
            Button btn = GetButton(i);
            GameObject layerObj = GetLayerObj(i);
            if (btn == null || layerObj == null) continue;

            int index = i;

            // Hover/Exit
            HoverHandler h = btn.GetComponent<HoverHandler>();
            if (h == null) h = btn.gameObject.AddComponent<HoverHandler>();
            h.Init(
                onEnter: () => { pointerOverIndex = index; HoverLayer(index); },
                onExit: () => { if (pointerOverIndex == index) pointerOverIndex = -1; ExitLayer(index); }
            );

            // Click
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ManageLayer(index));
        }
    }

    // =========================
    // CLICK: Toggle / özel durum
    // =========================
    public void ManageLayer(int index)
    {
        if (!HasValidPair(index)) return;

        Tilemap tm = GetTilemap(index);
        if (tm == null) return;

        // 1) Tilemap AÇIK ve dimmed iken tıklanırsa:
        // -> alpha 1 + tilemap KAPANIR
        if (toggledOn[index] && dimmedWhileOpen[index])
        {
            SetAlpha(tm, 1f);

            toggledOn[index] = false;
            dimmedWhileOpen[index] = false;
            previewOpened[index] = false;

            tm.gameObject.SetActive(false);

            // ✅ KAPATILDI AMA POINTER HÂLÂ BU BUTTON ÜSTÜNDEYSE:
            // Unity yeni PointerEnter göndermeyeceği için manuel hover çağır
            if (IsPointerStillOverThisButton(index))
                HoverLayer(index);

            return;
        }

        // 2) Normal toggle
        bool wantOpen = !toggledOn[index];

        if (wantOpen)
        {
            // Kapalıyken açılacak: player tile'larla çakışıyorsa açma
            if (playerCollider != null && IsPlayerOverlappingTilemap(tm, playerCollider))
            {
                // preview state’ine dokunma, exit düzgün kapatsın
                return;
            }

            toggledOn[index] = true;
            previewOpened[index] = false;
            dimmedWhileOpen[index] = false;

            RestoreToNormal(tm); // aktif + collider açık + alpha 1
        }
        else
        {
            toggledOn[index] = false;
            previewOpened[index] = false;
            dimmedWhileOpen[index] = false;

            tm.gameObject.SetActive(false);

            // ✅ kapatınca pointer hâlâ üstündeyse hover preview devam etsin
            if (IsPointerStillOverThisButton(index))
                HoverLayer(index);
        }
    }

    // =========================
    // HOVER
    // =========================
    public void HoverLayer(int index)
    {
        if (!HasValidPair(index)) return;

        Tilemap tm = GetTilemap(index);
        if (tm == null) return;

        // Tilemap AÇIKKEN: collision'a dokunma, sadece alpha 0.5
        if (toggledOn[index])
        {
            if (dimmedWhileOpen[index]) return;

            dimmedWhileOpen[index] = true;
            SetAlpha(tm, 0.5f);
            return;
        }

        // Tilemap KAPALIYKEN: preview modu
        if (previewOpened[index]) return;

        previewOpened[index] = true;
        ActivatePreview(tm);
    }

    // =========================
    // EXIT
    // =========================
    public void ExitLayer(int index)
    {
        if (!HasValidPair(index)) return;

        Tilemap tm = GetTilemap(index);
        if (tm == null) return;

        // Tilemap AÇIKKEN dimmed olduysa ve tıklanmadıysa:
        // -> Exit'te alpha 1'e dön
        if (toggledOn[index] && dimmedWhileOpen[index])
        {
            dimmedWhileOpen[index] = false;
            SetAlpha(tm, 1f);
            return;
        }

        // Tilemap KAPALIYKEN preview açıksa:
        // -> Exit'te preview kapat
        if (!toggledOn[index] && previewOpened[index])
        {
            previewOpened[index] = false;
            RestorePreviewAndDisable(tm);
        }
    }

    // =========================
    // Pointer hâlâ bu button üstünde mi?
    // (New Input System uyumlu)
    // =========================
    private bool IsPointerStillOverThisButton(int index)
    {
        // EventSystem pointer'ın UI üzerinde olup olmadığını bilir (mouse/touch/pen hepsi)
        if (EventSystem.current == null) return false;
        if (!EventSystem.current.IsPointerOverGameObject()) return false;

        // En son hover aldığımız index hâlâ aynıysa "üstünde" varsay
        return pointerOverIndex == index;
    }

    // =========================
    // Null-safe helpers
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

    private bool HasValidPair(int index) => GetButton(index) != null && GetLayerObj(index) != null;

    private Tilemap GetTilemap(int index)
    {
        GameObject obj = GetLayerObj(index);
        if (obj == null) return null;
        return obj.GetComponent<Tilemap>();
    }

    // =========================
    // Tilemap modları
    // =========================
    private static void ActivatePreview(Tilemap tilemap)
    {
        tilemap.gameObject.SetActive(true);

        var col = tilemap.GetComponent<TilemapCollider2D>();
        if (col) col.enabled = false;

        var comp = tilemap.GetComponent<CompositeCollider2D>();
        if (comp) comp.enabled = false;

        SetAlpha(tilemap, 0.5f);
    }

    private static void RestoreToNormal(Tilemap tilemap)
    {
        tilemap.gameObject.SetActive(true);

        var col = tilemap.GetComponent<TilemapCollider2D>();
        if (col) col.enabled = true;

        var comp = tilemap.GetComponent<CompositeCollider2D>();
        if (comp) comp.enabled = true;

        SetAlpha(tilemap, 1f);
    }

    private static void RestorePreviewAndDisable(Tilemap tilemap)
    {
        var col = tilemap.GetComponent<TilemapCollider2D>();
        if (col) col.enabled = true;

        var comp = tilemap.GetComponent<CompositeCollider2D>();
        if (comp) comp.enabled = true;

        SetAlpha(tilemap, 1f);
        tilemap.gameObject.SetActive(false);
    }

    private static void SetAlpha(Tilemap tilemap, float a)
    {
        Color c = tilemap.color;
        c.a = a;
        tilemap.color = c;
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

        public void Init(System.Action onEnter, System.Action onExit)
        {
            this.onEnter = onEnter;
            this.onExit = onExit;
        }

        public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke();
    }
}
