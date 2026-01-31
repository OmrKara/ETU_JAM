using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MaskDisplayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // [SerializeField] private int maskAmount = 3;
    [SerializeField] private Mask[] masks;

    [SerializeField] private Sprite frame;

    [SerializeField] private GameObject[] buttons;

    private GameObject[] activeButtons;
    public int layerMaskAmount;

    void Start()
    {
        activeButtons = new GameObject[layerMaskAmount];
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < layerMaskAmount)
            {
                activeButtons[i] = buttons[i];
            }
            else
            {
                buttons[i].SetActive(false);
            }
        }
    }

    void Update()
    {

    }
}
