using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DPipe : MonoBehaviour
{
    public static DPipe CURRENT_DPIPE;

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetButtonDown("Action"))
        {
            StartCoroutine(ClimbPipe(other.GetComponent<PlayerController>()));
        }
    }

    private IEnumerator ClimbPipe(PlayerController player)
    {
        player.MoveWait(transform.position - transform.forward * 0.3f, Quaternion.LookRotation(transform.forward),
            0.4f, 16f);

        while (player.isMovingAuto)
        {
            yield return null;
        }

        CURRENT_DPIPE = this;

        player.StateMachine.GoToState<Drainpipe>();
    }
}
