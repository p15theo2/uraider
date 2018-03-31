using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int maxItems = 14;

    private List<InventoryItem> items = new List<InventoryItem>();

    public List<InventoryItem> Items
    {
        get { return items; }
    }
}
