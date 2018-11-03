using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryItem : MonoBehaviour
{
    public string itemName;
    public bool destroyOnUse;

    public Sprite sprite;

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            if (Input.GetButtonDown("Action"))
            {
                PlayerInventory inventory =  other.gameObject.GetComponent<PlayerInventory>();
                Animator anim = other.gameObject.GetComponent<Animator>();

                inventory.addItem(this);
                anim.SetTrigger("PickUp");
                this.gameObject.SetActive(false);
            }
        }
    }

    public virtual void Use(PlayerController player)
    {

    }
}
