using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HurtZone : MonoBehaviour
{
    public int amount = 1;

    private PlayerStats stats;

    void Start()
    {
        stats = GameObject.FindObjectOfType<PlayerStats>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stats.Health -= amount;
        }
    }
}
