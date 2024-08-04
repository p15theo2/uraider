using UnityEngine;

public class RotateDoor : Door {
    private float tarRot;
    private float curRot;
    public Transform realDoor;
    // Use this for initialization
    public override void OpenDoor(PlayerController player)
    {
        player.gameObject.GetComponent<RingMenu>().EnableKeyMenu(this);
    }

    public override void OpenDoorAct()
    {
        if(keyName!=curKeyName) return;
        tarRot = 100f;
    }

    private void Update()
    {
        if (Mathf.Abs(curRot - tarRot) > 5f)
        {
            realDoor.transform.Rotate(0f, 0f, Mathf.Sign(tarRot - curRot) * Time.deltaTime * 30f);
            curRot += Mathf.Sign(tarRot - curRot) * Time.deltaTime * 30f;
        }
    }
}
