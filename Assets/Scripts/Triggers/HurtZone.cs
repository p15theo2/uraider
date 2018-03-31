using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HurtZone : MonoBehaviour
{
    public int amount = 1;

    private PlayerStats stats;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stats = other.gameObject.GetComponent<PlayerStats>();
            stats.ShowCanvas();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stats.Health -= amount;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stats.HideCanvas();
        }
    }
}
