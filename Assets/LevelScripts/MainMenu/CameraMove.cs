using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    Quaternion startRot;

	void Awake ()
    {
        startRot = transform.rotation;
	}
	
	void Update ()
    {
		if (Input.mousePosition.x > 0f && Input.mousePosition.x < Screen.width
            && Input.mousePosition.y > 0f && Input.mousePosition.y < Screen.height)
        {
            transform.rotation = 
                Quaternion.Euler(startRot.eulerAngles.x + ((Input.mousePosition.y - Screen.height / 2) * 10f / Screen.height),
                startRot.eulerAngles.y + ((Input.mousePosition.x - Screen.height / 2) * 10f / Screen.width),
                startRot.eulerAngles.z + 0f);
        }
	}
}
