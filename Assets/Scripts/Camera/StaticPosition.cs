using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticPosition : MonoBehaviour
{
    public CameraController camControl;
    public CameraCollision camCollision;
    public Camera cam;
    public Transform refPos;

    private Vector3 point;
    private Quaternion rotation;

    private void Start()
    {
        point = refPos.position;
        rotation = refPos.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        camCollision.enabled = false;
        camControl.State = CameraState.Freeze;

        cam.transform.position = point;
        cam.transform.rotation = rotation;
    }

    private void OnTriggerExit(Collider other)
    {
        camCollision.enabled = true;
        camControl.State = CameraState.Grounded;
    }
}
