using System.Linq;
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
    public Text combineable;
    public Text targetText;
    
    private float angleChange = 90f;

    private PlayerInput input;
    private bool isActive;
    private bool isDoorOpen;
    private bool isCombining;
    private int curCombineIndex;

    private string combineItemName;

    public Transform[] combinePoints;
    public GameObject combine;
    public int combineItemType;
    public int combineItemIndex;

    [HideInInspector]
    public Door targetDoor;

    private void Start()
    {   
        input = GetComponent<PlayerInput>();
        player = GetComponent<PlayerController>();
        Cursor.visible = false;
        menu.SetActive(false);
    }

    public void EnableKeyMenu(Door curDoor)
    {
        targetDoor = curDoor;
        curIndex = 0;
        curItemIndex = 0;
        isDoorOpen = true;
        isPaused = true;
        isActive = true;
        Input.ResetInputAxes();
        menu.SetActive(true);
        RefreshMenu();
        UpdateUI();
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
                    DisableMenu();
                }
            }
        }
        if(!isPaused) return;
        float hAxis = Input.GetAxisRaw(input.horizontalAxis);
        float vAxis = Input.GetAxisRaw(input.verticalAxis);
        if(Input.GetKeyDown(input.crouch) && isActive && inventory.Items[curIndex][curItemIndex].item.GetType().FullName == "CombineItem")
        {
            combineItemName = inventory.Items[curIndex][curItemIndex].item.itemName;
            if (!IsPossibleCombine())
            {
                combineable.text = "There is no item to combine.";
                combineItemName = "";
                return;
            }
            isCombining = true;
            curCombineIndex = 0;
            while (inventory.CombineItems()[curCombineIndex].item.itemName == combineItemName)
            {
                curCombineIndex += 1;
            }
            UpdateUI();
        }
        if (isCombining)
        {
            if(Mathf.Abs(hAxis) > 0.3f){
                curCombineIndex += Sign(hAxis);
                curCombineIndex %= inventory.CombineItems().Count;
                while (inventory.CombineItems()[curCombineIndex].item.itemName == combineItemName)
                {
                    curCombineIndex += Sign(hAxis);
                    curCombineIndex %= inventory.CombineItems().Count;
                    if (curCombineIndex < 0) curCombineIndex += inventory.CombineItems().Count;
                }
                UpdateUI();
            }
            if (Input.GetKey(input.action))
            {
                if(inventory.CombineItems()[curCombineIndex].item.itemName == ((CombineItem)(inventory.Items[curIndex][curItemIndex].item)).combineName)
                {
                    InventoryItem addItem = ((CombineItem)(inventory.Items[curIndex][curItemIndex].item)).result.GetComponent<InventoryItem>();
                    string name = inventory.CombineItems()[curCombineIndex].item.itemName;
                    inventory.RemoveItem(curItemIndex, curIndex);
                    inventory.RemoveItem(name);
                    isCombining = false;
                    isActive = true;
                    curItemIndex = 0;
                    curCombineIndex = 0;
                    inventory.AddFirstItem(addItem);
                    RefreshMenu();
                    UpdateUI();
                    return;
                }
            }
        }
        if (Input.GetKeyDown(input.action) && isActive && !isCombining)
        {
            if (inventory.Items[curIndex][curItemIndex].item.GetType().FullName == "CombineItem")
                return;
            if (inventory.Items[curIndex][curItemIndex].item.inventoryModel.GetComponent<Animator>())
                inventory.Items[curIndex][curItemIndex].item.inventoryModel.GetComponent<Animator>().SetTrigger("Confirm");
            isActive = false;
            if (curIndex != 0)
                inventory.Items[curIndex][curItemIndex].item.Use(player);
            else
            {
                if (targetDoor == null) return;
                targetDoor.curKeyName = inventory.Items[curIndex][curItemIndex].item.itemName;
            }
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
        if (!rotaters[curIndex].GetComponent<Ring>().moving && isActive && !isCombining)
        {
            if (Mathf.Abs(hAxis) > 0.3f && inventory.Items[curIndex].Count > 0)
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

    private bool IsPossibleCombine()
    {
        foreach(ItemInfo item in inventory.CombineItems())
        {
            if(item.item.name != combineItemName) return true;
        }
        return false;
    }

    private void HandleUse()
    {
        if (curIndex == 0 && isDoorOpen)
        {
            player.transform.position = targetDoor.openPoint.position;
            player.transform.rotation = targetDoor.openPoint.rotation;
            player.Anim.SetBool("DoorOpen", true);
            player.Anim.SetTrigger(targetDoor.charAnimTrigger);
            targetDoor.Invoke("OpenDoorAct", 3f);
            targetDoor = null;
        }
        curItemIndex = 0;
        curIndex = 1;
        isPaused = false;
        isDoorOpen = false;
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
        curItemIndex = 0;
        curIndex = 1;
        isDoorOpen = false;
        menu.SetActive(false);
        combineItemName = null;
    }

    private void UpdateUI()
    {
        if (inventory.Items[curIndex].Count == 0)
        {
            itemName.text = itemCount.text = combineable.text = targetText.text = "";
            return;
        }
        if (inventory.Items[curIndex][curItemIndex].item.GetType().FullName == "CombineItem") combineable.text = isCombining? "Combine" : "Combine With";
        else combineable.text = "";
        if (isCombining)
        {
            targetText.text = inventory.CombineItems()[curCombineIndex].item.itemName;
        }
        else
        {
            targetText.text = "";
        }
        itemName.text = inventory.Items[curIndex][curItemIndex].item.itemName;
        itemCount.text = inventory.Items[curIndex][curItemIndex].count == 1? "" : inventory.Items[curIndex][curItemIndex].count.ToString();
    }

    private void RefreshMenu()
    {
        for (int index = 0; index < 3; index++)
        {
            rotaters[index].transform.position = subRingTransforms[index].transform.position + new Vector3(0f, (curIndex - 1) * 5f, 0f);
            rotaters[index].transform.rotation = subRingTransforms[index].transform.rotation;
            foreach (Transform child in rotaters[index])
            {
                Destroy(child.gameObject);
            }
            int itemCount = inventory.Items[index].Count;
            angleChange = (2f * Mathf.PI) / itemCount;
            for (int i = 0; i < itemCount; i++)
            {
                GameObject item = Instantiate(inventory.Items[index][i].item.inventoryModel, rotaters[index]);

                float angle = angleChange * i;
                float x = (index == curIndex ? 2f : 0f) * Mathf.Sin(angle);  // Convert polar co-ords to cartesian
                float z = (index == curIndex ? 2f : 0f) * Mathf.Cos(angle);

                item.transform.localPosition = new Vector3(x, 0, z);
                item.transform.eulerAngles = new Vector3 (0f, 180f + i * 360f / itemCount, 0f);
                foreach (Transform child in item.transform)
                {
                    child.gameObject.layer = rotaters[index].gameObject.layer;
                }
                item.layer = rotaters[index].gameObject.layer;
            }
        }
    }
}
