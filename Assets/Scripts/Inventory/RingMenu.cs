using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class RingMenu : MonoBehaviour
{
    public static bool isPaused = false;

    public GameObject menu;
    public Transform[] rotaters;
    public PlayerInventory inventory;
    public PlayerController player;
    public int curIndex = 1;
    public int curItemIndex = 0;
    public Transform[] subRingTransforms;
    public Text itemName;
    public Text itemCount;
    
    private float angleChange = 90f;

    private PlayerInput input;
    private bool isActive;

    private void Start()
    {   
        input = GetComponent<PlayerInput>();
        player = GetComponent<PlayerController>();
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
                if (isActive)
                {
                    curItemIndex = 0;
                    curIndex = 1;
                    DisableMenu();
                }
            }
        }
        if(!isPaused) return;
        float hAxis = Input.GetAxisRaw(input.horizontalAxis);
        float vAxis = Input.GetAxisRaw(input.verticalAxis);
        if (Input.GetKeyDown(input.action) && isActive)
        {
            if (inventory.Items[curIndex][curItemIndex].item.inventoryModel.GetComponent<Animator>())
                inventory.Items[curIndex][curItemIndex].item.inventoryModel.GetComponent<Animator>().SetTrigger("Confirm");
            isActive = false;
            inventory.Items[curIndex][curItemIndex].item.Use(player);
            if (inventory.Items[curIndex][curItemIndex].item.destroyOnUse)
            {
                if (inventory.Items[curIndex][curItemIndex].count > 1)
                {
                    inventory.Items[curIndex][curItemIndex].count--;
                }
                else
                {
                    inventory.RemoveItem(curItemIndex, curIndex);
                }
            }
            Invoke("HandleUse", 3f);
        }
        if (!rotaters[curIndex].GetComponent<Ring>().moving && isActive)
        {
            if (Mathf.Abs(hAxis) > 0.3f)
            {
                rotaters[curIndex].GetComponent<Ring>().RotateOneStep(Sign(hAxis));
                curItemIndex += Sign(hAxis);
                curItemIndex %= inventory.Items[curIndex].Count;
                if (curItemIndex < 0) curItemIndex += inventory.Items[curIndex].Count;
                Input.ResetInputAxes();
                UpdateUI();
                return;
            }
            if (Mathf.Abs(vAxis) > 0.3f)
            {
                Input.ResetInputAxes();
                if (vAxis < 0f && curIndex == 0) return ;
                if(vAxis > 0f && curIndex == 2) return ;
                int direction = Sign(vAxis);
                curIndex += direction;
                curItemIndex = 0;
                int type = 0;
                for(int i = 0; i < rotaters.Length; i++)
                {
                    if (i == curIndex - direction) type = 1;
                    else if (i == curIndex) type = 2;
                    rotaters[i].GetComponent<Ring>().Move(direction, type);
                }
                UpdateUI();
            }
        }
    }

    private void HandleUse()
    {
        isPaused = false;
        menu.SetActive(false);
    }

    private int Sign(float val)
    {
        if (val > 0) return 1;
        else if(val < 0) return -1;
        return 0;
    }

    private void EnableMenu()
    {
        isActive = true;
        menu.SetActive(true);
        RefreshMenu();
        UpdateUI();
    }

    private void DisableMenu()
    {
        if (rotaters[curIndex].GetComponent<Ring>().moving)
            return;
        menu.SetActive(false);
    }

    private void UpdateUI()
    {
        if (inventory.Items[curIndex].Count == 0)
        {
            itemName.text = itemCount.text = "";
            return;
        }
        itemName.text = inventory.Items[curIndex][curItemIndex].item.itemName;
        itemCount.text = inventory.Items[curIndex][curItemIndex].count == 1? "" : inventory.Items[curIndex][curItemIndex].count.ToString();
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
            int itemCount = inventory.Items[index].Count;
            angleChange = (2f * Mathf.PI) / itemCount;
            int curIndex = 0;
            for (int i = 0; i < itemCount; i++)
            {
                GameObject item = Instantiate(inventory.Items[index][i].item.inventoryModel, rotaters[index]);

                float angle = angleChange * curIndex;
                float x = (index == 1 ? 2f : 0f) * Mathf.Sin(angle);  // Convert polar co-ords to cartesian
                float z = (index == 1 ? 2f : 0f) * Mathf.Cos(angle);

                item.transform.localPosition = new Vector3(x, 0f, z);
                item.transform.eulerAngles = new Vector3 (0f, 180f + curIndex * 360f / itemCount, 0f);
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
