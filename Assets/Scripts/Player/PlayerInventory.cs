using System.Collections.Generic;
using UnityEngine;

public class ItemInfo
{
    public InventoryItem item;
    public int count;

    public ItemInfo(InventoryItem pItem, int pCount) 
    {
        item = pItem;
        count = pCount;
    }
};

public class PlayerInventory : MonoBehaviour
{
    public const int maxItems = 14;
    private bool activeGUI = false;

    private GameObject itemsUI;
    private List<ItemInfo>[] items= new List<ItemInfo>[3];
    public int[,] itemNum = new int[maxItems,2];

    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            items[i] = new List<ItemInfo>();
        }
        InventoryItem[] menuItems = gameObject.GetComponents<InventoryItem>();
        for(int i = 0; i< menuItems.Length; i++)
        {
            items[2].Add(new ItemInfo(menuItems[i], 1));
        }
    }

    public void AddItem(InventoryItem item)
    {
        int index = item.type;
        for (int i = 0; i < items[index].Count; i++)
        {
            if (items[index][i].item.itemName == item.itemName)
            {
                items[index][i].count++;
                return;
            }
        }
        items[index].Add(new ItemInfo(item, 1));
    }

    public void RemoveItem(int index, int type)
    {
        items[type].RemoveAt(index);
    }

    public List<ItemInfo>[] Items
    {
        get { return items; }
    }
}
