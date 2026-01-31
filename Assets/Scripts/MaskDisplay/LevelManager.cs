using UnityEngine;

public class LevelManager : MonoBehaviour
{

    [SerializeField] Level level;
    [SerializeField] PortalScript portal;
    [SerializeField] MaskDisplayer maskDisplayer;

    void Start()
    {
        portal.levelMaskAmount = level.itemAmount;
        portal.levelNum = level.levelNum;
        maskDisplayer.layerMaskAmount = level.layerMaskAmount;
    }

    void Update()
    {

    }
}
