using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerInventory : MonoBehaviour
{
    public const int maxItems = 14;
    public int[] itemCount = new int[3];
    private InventoryItem[] menuItems;
    private bool activeGUI = false;

    private GameObject itemsUI;
    private InventoryItem[,] items = new InventoryItem[maxItems,3];

    private void Start()
    {
        for (int i = 0; i < maxItems; i++)
        {
            items[i, 0] = null;
            items[i, 1] = null;
            items[i, 2] = null;
        }
        itemCount[0] = 0;
        itemCount[1] = 0;
        itemCount[2] = 0;

        menuItems = gameObject.GetComponents<InventoryItem>();
        for(int i = 0; i< menuItems.Length; i++)
        {
            items[i, 2] = menuItems[i];
        }
        itemCount[2] = menuItems.Length;
    }

    private void Update()
    {
        
    }

    public void AddItem(InventoryItem item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i, item.type] == null)
            {
                items[i, item.type] = item;
                itemCount[item.type]++;
                return;
            }
        }
    }

    public void RemoveItem(InventoryItem item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i, item.type] == item)
            {
                items[i, item.type] = null;
                itemCount[item.type]--;
                return;
            }
        }
    }

    public void RemoveItem(int i, int type)
    {
        items[i, type] = null;
    }

    public InventoryItem[,] Items
    {
        get { return items; }
    }
}
