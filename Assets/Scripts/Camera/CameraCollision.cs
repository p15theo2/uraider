using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public float zoomSpeed = 20.0f;
    public float retreatSpeed = 14.0f;
    public float sphereRadius = 0.1f;

    private RaycastHit hit;
    private Transform cam;
    private Transform pivot;

    private float initDist;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>().transform;
        pivot = cam.transform.parent;
        initDist = Vector3.Distance(pivot.transform.position, cam.transform.position);
    }

    private void LateUpdate()
    {
        Vector3 rayStart = pivot.transform.position;
        Vector3 dir = -pivot.transform.forward;

        if (Physics.SphereCast(rayStart, sphereRadius, dir, out hit, initDist))
        {
            if (hit.transform.tag != "Player" && hit.transform.tag != "MainCamera")
            {
                float pointOffset = (hit.point - pivot.position).magnitude;
                cam.localPosition = Vector3.Slerp(cam.localPosition, cam.localPosition.normalized * pointOffset, Time.deltaTime * zoomSpeed);
                return;
            }
        }

        cam.localPosition = Vector3.Slerp(cam.localPosition, cam.localPosition.normalized * initDist, Time.deltaTime * retreatSpeed);
    }
}
