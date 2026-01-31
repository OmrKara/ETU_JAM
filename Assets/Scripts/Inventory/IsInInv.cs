using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class IsInInv : MonoBehaviour
{
    ItemSO itemSo;
    public Image image;
    public bool isInv;
   
    public void UpdateUI()
    {
        image.sprite = itemSo.icon;
        image.gameObject.SetActive(false);
        if (itemSo != null)
        {
            image.gameObject.SetActive (true);
        }
    }
}
