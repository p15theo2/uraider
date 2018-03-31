using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryItem : MonoBehaviour
{
    public string itemName;

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayerInventory inventory =  other.gameObject.GetComponent<PlayerInventory>();
                Animator anim = other.gameObject.GetComponent<Animator>();

                inventory.Items.Add(this);
                anim.SetTrigger("PickUp");
                Destroy(this.gameObject);
            }
        }
    }
}
