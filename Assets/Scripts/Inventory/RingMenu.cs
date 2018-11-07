using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public float rotationRate = 10f;

    public GameObject menu;
    public Transform rotater;
    public PlayerInventory inventory;

    private float angleChange = 90f;

    private Quaternion targetRotation;
    private PlayerInput input;

    private void Start()
    {
        input = GetComponent<PlayerInput>();
        Cursor.visible = false;
        menu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(input.inventory))
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
            if (Mathf.Abs(Input.GetAxisRaw(input.horizontalAxis)) > 0.3f)
                RotateTo(Mathf.Sign(Input.GetAxisRaw(input.horizontalAxis)) * angleChange * Mathf.Rad2Deg);
        }

        if (Mathf.Abs(targetRotation.eulerAngles.y - rotater.eulerAngles.y) > 4f)
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
        RefreshMenu();
    }

    private void DisableMenu()
    {
        menu.SetActive(false);
    }

    private void RefreshMenu()
    {
        foreach (Transform child in rotater)
        {
            Destroy(child.gameObject);
        }

        angleChange = (2f * Mathf.PI) / inventory.itemCount;

        Debug.Log(inventory.itemCount + " " + angleChange);

        for (int i = 0; i < inventory.Items.Length; i++)
        {
            if (inventory.Items[i] == null)
                continue;

            GameObject item = Instantiate(inventory.Items[i].inventoryModel, rotater);

            float angle = angleChange * (i + 1);
            float x = 2f * Mathf.Sin(angle);  // Convert polar co-ords to cartesian
            float z = -2f * Mathf.Cos(angle);

            item.transform.localPosition = new Vector3(x, 0f, z);
            foreach (Transform child in item.transform)
            {
                child.gameObject.layer = rotater.gameObject.layer;
            }
            item.layer = rotater.gameObject.layer;
        }

        
    }
}
