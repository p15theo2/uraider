using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool openLeft = true;
    public bool pull = true;

	public void Open()
    {
        
    }

    private void OnTriggerStay(Collider col)
    {
        if (Input.GetButtonDown("Action") && col.CompareTag("Player"))
        {
            StartCoroutine(OpenDoor(col.GetComponent<PlayerController>()));
        }
    }

    private IEnumerator OpenDoor(PlayerController player)
    {
        player.MoveWait(transform.position + Vector3.forward * 1f + Vector3.left * 0.34f, Quaternion.LookRotation(Vector3.right),
            0.4f, 16f);

        while (player.isMovingAuto)
        {
            yield return null;
        }

        player.Anim.SetTrigger("PullDoorLeft");
        GetComponent<Animator>().Play("PullOnLeft");
        player.Anim.applyRootMotion = true;

        AnimatorStateInfo stateInfo = player.Anim.GetCurrentAnimatorStateInfo(0);
        do
        {
            stateInfo = player.Anim.GetCurrentAnimatorStateInfo(0);
            yield return null;
        } while (stateInfo.IsName("Locomotion"));

        player.Anim.applyRootMotion = false;
        GetComponent<BoxCollider>().enabled = false;
    }
}
