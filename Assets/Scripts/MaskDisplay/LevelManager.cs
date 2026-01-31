using UnityEngine;

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
}
