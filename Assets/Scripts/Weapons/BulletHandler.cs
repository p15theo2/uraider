using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    public int damage;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
