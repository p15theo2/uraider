using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLight : MonoBehaviour
{
    private bool isOn = false;

    private GameObject spotLight;

    private void Start()
    {
        spotLight = transform.GetChild(0).gameObject;
        spotLight.SetActive(isOn);
    }

    private void Update()
    {
        if (Input.GetKeyDown(GetComponentInParent<PlayerInput>().pls))
        {
            isOn = !isOn;
            spotLight.SetActive(isOn);
        }
    }
}
