using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{

    [SerializeField] Level level;
    [SerializeField] PortalScript portal;
    [SerializeField] MaskDisplayer maskDisplayer;
    [SerializeField] private GameObject[] layers;

    void Start()
    {
        portal.levelMaskAmount = level.itemAmount;
        portal.levelNum = level.levelNum;
        maskDisplayer.layerMaskAmount = level.layerMaskAmount;

        for (int i = 0; i < level.layerMaskAmount; i++)
        {
            layers[i].SetActive(false);
        }
    }

    void Update()
    {

    }

    public void ManageLayer(int ButtonIndex)
    {
        GameObject layer = layers[ButtonIndex];
        if (layer.activeSelf)
        {
            layer.SetActive(false);
        }
        else
        {
            layer.SetActive(true);
        }
    }

    public void DisplayLayer(int ButtonIndex)
    {
        GameObject layer = layers[ButtonIndex];
        if (!layer.activeSelf)
        {
            layer.SetActive(false);
            layer.GetComponent<CompositeCollider2D>().enabled = false;
            layer.GetComponent<Tilemap>();

        }
    }

    public static void ActivateTilemapWithoutCollision(Tilemap tilemap)
    {
        if (tilemap == null) return;

        tilemap.gameObject.SetActive(true);

        // Collision kapat
        var collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null) collider.enabled = false;

        var composite = tilemap.GetComponent<CompositeCollider2D>();
        if (composite != null) composite.enabled = false;

        // Alpha %50 (DOÐRU YER)
        Color c = tilemap.color;
        c.a = 0.5f;
        tilemap.color = c;
    }
    public static void RestoreTilemap(Tilemap tilemap)
    {
        if (tilemap == null) return;

        tilemap.gameObject.SetActive(true);

        // Collision aç
        var collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null) collider.enabled = true;

        var composite = tilemap.GetComponent<CompositeCollider2D>();
        if (composite != null) composite.enabled = true;

        // Alpha %100
        Color c = tilemap.color;
        c.a = 1f;
        tilemap.color = c;
    }

    }
