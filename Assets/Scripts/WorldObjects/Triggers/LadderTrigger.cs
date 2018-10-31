using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderTrigger : MonoBehaviour
{
    public bool isSide = false;

    private bool starting = false;

    void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("Player") 
            && !col.gameObject.GetComponent<PlayerController>().StateMachine.IsInState<Locomotion>())
            starting = true;

        if (!starting && col.CompareTag("Player") && Vector3.Dot(transform.forward, col.transform.forward) > 0f)
        {
            if (!col.gameObject.GetComponent<PlayerController>().StateMachine.IsInState<Locomotion>())
                return;
            ClimbLadder(col.gameObject.GetComponent<PlayerController>());
            starting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        starting = false;
    }

    private void ClimbLadder(PlayerController player)
    {
        player.Anim.applyRootMotion = false;

        LadderVolume.CURRENT_LADDER = transform.parent.gameObject.GetComponent<LadderVolume>();

        player.Anim.SetTrigger(isSide ? "LadderSide" : "LadderFront");

        player.StateMachine.GoToState<Ladder>();
    }
}
