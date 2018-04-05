using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public const int maxItems = 14;

    public Canvas canvas;
    public GameObject itemsHolder;
    public GameObject itemPrefab;

    private bool activeGUI = false;

    private GameObject itemsUI;
    private InventoryItem[] items = new InventoryItem[maxItems];

    private void Start()
    {
        itemsUI = canvas.transform.Find("Items").gameObject;
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            activeGUI = !activeGUI;

            if (activeGUI)
            {
                Time.timeScale = 0f;
                canvas.enabled = true;

                foreach (InventoryItem item in items)
                {
                    if (item == null)
                        continue;

                    GameObject itemUI = Instantiate(itemPrefab);

                    itemUI.transform.SetParent(itemsHolder.transform, false);
                    itemUI.GetComponent<ItemUIEditor>().SetImage(item.sprite);
                    itemUI.GetComponent<ItemUIEditor>().SetText(item.itemName);
                }
            }
            else
            {
                Time.timeScale = 1f;
                canvas.enabled = false;
            }
        }

        if (!activeGUI)
        {
            return;
        }
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
