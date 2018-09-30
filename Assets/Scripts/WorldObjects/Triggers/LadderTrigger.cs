using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderTrigger : MonoBehaviour
{
    public static LadderTrigger CURRENT_LADDER;

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetButtonDown("Action"))
        {
            StartCoroutine(ClimbLadder(other.GetComponent<PlayerController>()));
        }
    }

    private IEnumerator ClimbLadder(PlayerController player)
    {
        player.MoveWait(transform.position - transform.forward * 0.4f, Quaternion.LookRotation(transform.forward),
            0.4f, 16f);

        while (player.isMovingAuto)
        {
            yield return null;
        }

        CURRENT_LADDER = this;

        player.StateMachine.GoToState<Ladder>();
    }
}
