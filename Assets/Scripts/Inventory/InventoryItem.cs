using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryItem : MonoBehaviour
{
    public string itemName;
    public bool destroyOnUse;

    public Sprite sprite;
    public GameObject inventoryModel;

    private PlayerInventory inventory;
    private PickUICam pickCam;

    private void Start()
    {
        pickCam = GameObject.FindObjectOfType<PickUICam>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            if (Input.GetButtonDown("Action"))
            {
                inventory =  other.gameObject.GetComponent<PlayerInventory>();
                Animator anim = other.gameObject.GetComponent<Animator>();
                anim.SetTrigger("PickUp");
                Invoke("PickedItem", 1.7f);
            }
        }
    }

    private void PickedItem()
    {
        inventory.AddItem(this);
        pickCam.SetAndEnable(gameObject);
        gameObject.SetActive(false);
    }

    public abstract void Use(PlayerController player);
}
