using System;
using UnityEngine;

public class RingMenu : MonoBehaviour
{
    public static bool isPaused = false;

    public GameObject menu;
    public Transform[] rotaters;
    public PlayerInventory inventory;

    public int curIndex = 1;
    public Transform[] subRingTransforms;
    
    private float angleChange = 90f;

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
            {
                EnableMenu();
                Input.ResetInputAxes();
            }
            else
            {
                DisableMenu();
                curIndex = 1;
            }
        }
        if(!isPaused) return;
        float hAxis = Input.GetAxisRaw(input.horizontalAxis);
        float vAxis = Input.GetAxisRaw(input.verticalAxis);
        if (!rotaters[curIndex].GetComponent<Ring>().moving)
        {
            if (Mathf.Abs(hAxis) > 0.3f)
            {
                rotaters[curIndex].GetComponent<Ring>().RotateOneStep(Sign(hAxis));
                Input.ResetInputAxes();
                return;
            }
            if (Mathf.Abs(vAxis) > 0.3f)
            {
                Input.ResetInputAxes();
                if (vAxis < 0f && curIndex == 0) return ;
                if(vAxis > 0f && curIndex == 2) return ;
                int direction = Sign(vAxis);
                curIndex += direction;
                int type = 0;
                for(int i = 0; i < rotaters.Length; i++)
                {
                    if (i == curIndex - direction) type = 1;
                    else if (i == curIndex) type = 2;
                    rotaters[i].GetComponent<Ring>().Move(direction, type);
                }
            }
        }
    }

    private int Sign(float val)
    {
        if (val > 0) return 1;
        else if(val < 0) return -1;
        return 0;
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
        for (int index = 0; index < 3; index++)
        {
            rotaters[index].transform.position = subRingTransforms[index].transform.position;
            rotaters[index].transform.rotation = subRingTransforms[index].transform.rotation;
            foreach (Transform child in rotaters[index])
            {
                Destroy(child.gameObject);
            }

            angleChange = (2f * Mathf.PI) / inventory.itemCount[index];
            int curIndex = 0;
            for (int i = 0; i < 14; i++)
            {
                if (inventory.Items[i, index] == null)
                    continue;
                GameObject item = Instantiate(inventory.Items[i, index].inventoryModel, rotaters[index]);

                float angle = angleChange * (curIndex + 1);
                float x = (index == 1 ? 2f : 0f) * Mathf.Sin(angle);  // Convert polar co-ords to cartesian
                float z = (index == 1 ? -2f : 0f) * Mathf.Cos(angle);

                item.transform.localPosition = new Vector3(x, 0f, z);
                item.transform.eulerAngles = new Vector3 (0f, -curIndex * 360f / inventory.itemCount[index], 0f);
                foreach (Transform child in item.transform)
                {
                    child.gameObject.layer = rotaters[index].gameObject.layer;
                }
                item.layer = rotaters[index].gameObject.layer;
                curIndex++;
            }
        }
    }
}
