using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Level level;
    [SerializeField] PortalScript portal;
    [SerializeField] MaskDisplayer maskDisplayer;
    [SerializeField] private GameObject[] layers;

    private bool[] layerToggledOn;   // click ile kalıcı açık mı?
    private bool[] previewOpened;    // hover ile preview açıldı mı?

    void Start()
    {
        portal.levelMaskAmount = level.itemAmount;
        portal.levelNum = level.levelNum;
        maskDisplayer.layerMaskAmount = level.layerMaskAmount;

        int n = level.layerMaskAmount;
        layerToggledOn = new bool[n];
        previewOpened = new bool[n];

        for (int i = 0; i < n; i++)
        {
            layerToggledOn[i] = false;
            previewOpened[i] = false;
            layers[i].SetActive(false);
        }
    }

    // =====================
    // CLICK: kalıcı toggle
    // =====================
    public void ManageLayer(int index)
    {
        Tilemap tm = layers[index].GetComponent<Tilemap>();
        if (tm == null) return;

        bool newState = !layerToggledOn[index];
        layerToggledOn[index] = newState;

        if (newState)
        {
            // ✅ Kalıcı açıldı: preview bayrağını temizle ki exit kapatamasın
            previewOpened[index] = false;

            RestoreToNormal(tm); // aktif + collider açık + alpha 1
        }
        else
        {
            // ✅ Kalıcı kapandı: preview bayrağını da temizle
            previewOpened[index] = false;

            tm.gameObject.SetActive(false);
        }
    }

    // =====================
    // HOVER: preview aç
    // =====================
    public void HoverLayer(int index)
    {
        if (layerToggledOn[index]) return;  // kalıcı açıksa hover yok
        if (previewOpened[index]) return;   // zaten preview açık

        Tilemap tm = layers[index].GetComponent<Tilemap>();
        if (tm == null) return;

        previewOpened[index] = true;
        ActivatePreview(tm);
    }

    // =====================
    // EXIT: preview kapat
    // =====================
    public void ExitLayer(int index)
    {
        if (layerToggledOn[index]) return;   // ✅ kalıcı açıksa asla kapatma
        if (!previewOpened[index]) return;   // preview açılmadıysa kapatma

        Tilemap tm = layers[index].GetComponent<Tilemap>();
        if (tm == null) return;

        previewOpened[index] = false;
        RestorePreviewAndDisable(tm);
    }

    // ---------------------
    // PREVIEW ON (hover)
    // ---------------------
    public static void ActivatePreview(Tilemap tilemap)
    {
        tilemap.gameObject.SetActive(true);

        var collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null) collider.enabled = false;

        var composite = tilemap.GetComponent<CompositeCollider2D>();
        if (composite != null) composite.enabled = false;

        Color c = tilemap.color;
        c.a = 0.5f;
        tilemap.color = c;
    }

    // ---------------------
    // NORMAL ON (click aç)
    // ---------------------
    private static void RestoreToNormal(Tilemap tilemap)
    {
        tilemap.gameObject.SetActive(true);

        var collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null) collider.enabled = true;

        var composite = tilemap.GetComponent<CompositeCollider2D>();
        if (composite != null) composite.enabled = true;

        Color c = tilemap.color;
        c.a = 1f;
        tilemap.color = c;
    }

    // ---------------------
    // PREVIEW OFF (exit)
    // ---------------------
    public static void RestorePreviewAndDisable(Tilemap tilemap)
    {
        // güvenli restore
        var collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null) collider.enabled = true;

        var composite = tilemap.GetComponent<CompositeCollider2D>();
        if (composite != null) composite.enabled = true;

        Color c = tilemap.color;
        c.a = 1f;
        tilemap.color = c;

        tilemap.gameObject.SetActive(false);
    }
}
