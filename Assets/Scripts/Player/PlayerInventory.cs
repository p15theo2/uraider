using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public const int maxItems = 14;

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

    public void addItem(InventoryItem item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                Debug.Log("Item added");
                items[i] = item;
                return;
            }
        }
    }

    public void removeItem(InventoryItem item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == item)
            {
                items[i] = null;
            }
        }
    }

    public void removeItem(int i)
    {
        items[i] = null;
    }

    public InventoryItem[] Items
    {
        get { return items; }
    }
}
