using UnityEngine;

public class LevelManager : MonoBehaviour
{

    [SerializeField] Level level;
    [SerializeField] PortalScript portal;

    void Start()
    {
        portal.levelMaskAmount = level.itemAmount;
        portal.levelNum = level.levelNum;
    }

    void Update()
    {

    }
}
