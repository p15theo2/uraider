using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 120.0f;
    public float yMax = 80.0f;
    public float yMin = -45.0f;
    public float rotationSmoothing = 30f;
    public float translationSmoothing = 30f;
    public bool LAUTurning = true;
    public bool isSplit = false;
    public string MouseX = "Mouse X";
    public string MouseY = "Mouse Y";

    [Header("Split Variables")]
    public Vector2 position;
    public Vector2 size;

    public Transform target;

    private float yRot = 0.0f;
    private float xRot = 0.0f;

    private Transform pivot;
    private Vector3 pivotOrigin;
    private Vector3 targetPivotPosition;
    private Vector3 forceDirection;
    private Camera cam;
    private CameraState camState;

    private void Start()
    {
        forceDirection = Vector3.zero;
        camState = CameraState.Grounded;
        cam = GetComponentInChildren<Camera>();
        if (isSplit)
            cam.rect = new Rect(position, size);
        pivot = cam.transform.parent;
        pivotOrigin = pivot.localPosition;
        targetPivotPosition = pivot.localPosition;
    }

    private void LateUpdate()
    {
        if (camState == CameraState.Freeze)
            return;

        HandleMovement();
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (camState == CameraState.Grounded)
            yRot += Input.GetAxis(MouseX) * rotationSpeed * Time.deltaTime;
        else
            yRot = target.rotation.eulerAngles.y;

        xRot -= Input.GetAxis(MouseY) * rotationSpeed * Time.deltaTime; // Negative so mouse up = cam down
        xRot = Mathf.Clamp(xRot, yMin, yMax);

        if (LAUTurning && camState == CameraState.Grounded 
            && Mathf.Abs(Input.GetAxis(MouseX)) == 0f)
            DoExtraRotation();

        Quaternion targetRot = forceDirection != Vector3.zero ? 
            Quaternion.LookRotation(forceDirection) 
            : Quaternion.Euler(xRot, yRot, 0.0f);

        if (rotationSmoothing != 0f)
            pivot.rotation = Quaternion.Slerp(pivot.rotation, targetRot, rotationSmoothing * Time.deltaTime);
        else
            pivot.rotation = targetRot;

        pivot.localPosition = Vector3.Lerp(pivot.localPosition, targetPivotPosition, Time.deltaTime * 2f);
    }

    private void HandleMovement()
    {
        if (translationSmoothing != 0f)
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * translationSmoothing);
        else
            transform.position = target.position;
    }

    private void DoExtraRotation()
    {
        yRot += 2.0f * Input.GetAxis(target.GetComponent<PlayerInput>().horizontalAxis);
    }

    public void PivotOnHead()
    {
        targetPivotPosition = Vector3.zero + Vector3.up * 1.7f;
    }

    public void PivotOnTarget()
    {
        targetPivotPosition = Vector3.zero;
    }

    public void PivotOnPivot()
    {
        targetPivotPosition = pivotOrigin;
    }

    public CameraState State
    {
        get { return camState; }
        set { camState = value; }
    }

    public Vector3 ForceDirection
    {
        get { return forceDirection; }
        set { forceDirection = value; }
    }
}

// enum incase of future extensions
public enum CameraState
{
    Freeze,
    Grounded,
    Combat
}
