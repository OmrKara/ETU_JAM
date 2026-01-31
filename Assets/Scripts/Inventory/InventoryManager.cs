using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public IsInInv[] isIN;

    public void Start()
    {
        foreach(var slot in isIN)
        {
            slot.UpdateUI();
        }
    }
    private void OnEnable()
    {
        Loot.OnItemLooted += AddItem;
    }

    public void AddItem(ItemSO itemSO , int quantity)
    {
       foreach (var slot in isIN)
        {
            slot.UpdateUI();
        }

    }
}
