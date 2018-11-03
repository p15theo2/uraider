using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public float rotationRate = 10f;

    public GameObject menu;
    public Transform rotater;

    private float angleChange = 90f;

    private Quaternion targetRotation;

    private void Start()
    {
        Cursor.visible = false;
        menu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isPaused = !isPaused;

            if (isPaused)
                EnableMenu();
            else
                DisableMenu();
        }

        if (!isPaused)
            return;
        
        if (rotater.rotation.eulerAngles.y == targetRotation.eulerAngles.y)
        {
            Debug.Log("Can rotate");
            if (Input.GetAxisRaw("Horizontal") != 0f)
                RotateTo(Mathf.Sign(Input.GetAxisRaw("Horizontal")) * angleChange);
        }

        if (Mathf.Abs(targetRotation.eulerAngles.y - rotater.eulerAngles.y) > 1f)
            rotater.rotation = Quaternion.Lerp(rotater.rotation, targetRotation, rotationRate * Time.deltaTime);
        else
            rotater.rotation = targetRotation;
    }

    private void RotateTo(float delta)
    {
        targetRotation = Quaternion.Euler(0f, rotater.rotation.eulerAngles.y + delta, 0f);
    }

    private void EnableMenu()
    {
        menu.SetActive(true);
    }

    private void DisableMenu()
    {
        menu.SetActive(false);
    }
}
