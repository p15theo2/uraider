using UnityEngine;

public abstract class Door : MonoBehaviour
{
    public string keyName;
    [HideInInspector]
    public string curKeyName;
    public string charAnimTrigger;
    public Transform openPoint;
    private bool openRequestSent =false;
    private void OnTriggerStay(Collider col)
    {
        if (Input.GetButtonUp("Action") && col.CompareTag("Player") && !openRequestSent)
        {
            OpenDoor(col.GetComponent<PlayerController>());
            openRequestSent = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            openRequestSent = false;
        }
    }

    public abstract void OpenDoor(PlayerController player);

    public abstract void OpenDoorAct();
}
