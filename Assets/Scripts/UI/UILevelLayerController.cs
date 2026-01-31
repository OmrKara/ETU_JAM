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

        toggledOn = new bool[n];
        previewOpened = new bool[n];

        // Başlangıçta kapat
        for (int i = 0; i < n; i++)
        {
            toggledOn[i] = false;
            previewOpened[i] = false;

            if (layers[i] != null)
                layers[i].SetActive(false);
        }

        // Butonlara hover/exit/click bağla
        for (int i = 0; i < n; i++)
        {
            int index = i;

            if (buttons[index] == null) continue;

            // Hover/Exit handler
            var handler = buttons[index].gameObject.GetComponent<HoverHandler>();
            if (handler == null) handler = buttons[index].gameObject.AddComponent<HoverHandler>();
            handler.Init(
                onEnter: () => HoverLayer(index),
                onExit: () => ExitLayer(index)
            );

            // Click
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
        toggledOn[index] = newState;

        if (newState)
        {
            // Kalıcı açıldı -> preview bayrağını temizle
            previewOpened[index] = false;
            RestoreToNormal(tm); // aktif + collider açık + alpha 1
        }
        else
        {
            // Kalıcı kapandı
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
        if (toggledOn[index]) return;      // kalıcı açıksa hover yok
        if (previewOpened[index]) return;  // zaten preview açık

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
        if (toggledOn[index]) return;        // ✅ kalıcı açıksa asla kapatma
        if (!previewOpened[index]) return;   // preview açılmadıysa kapatma

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

        // değerleri normale çek (güvenli)
        var collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null) collider.enabled = true;

        var composite = tilemap.GetComponent<CompositeCollider2D>();
        if (composite != null) composite.enabled = true;

        Color c = tilemap.color;
        c.a = 1f;
        tilemap.color = c;

        // preview kapat
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
