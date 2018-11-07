using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public const int maxItems = 14;
    [HideInInspector]
    public int itemCount = 0;

    private bool activeGUI = false;

    private GameObject itemsUI;
    private InventoryItem[] items = new InventoryItem[maxItems];

    private void Start()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = null;
        }
    }

    private void Update()
    {
        
    }

    public void AddItem(InventoryItem item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                itemCount++;
                return;
            }
        }
    }

    public void RemoveItem(InventoryItem item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;
                itemCount--;
                return;
            }
        }
    }

    public void RemoveItem(int i)
    {
        items[i] = null;
    }

    public InventoryItem[] Items
    {
        get { return items; }
    }
}
