using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DPipe : MonoBehaviour
{
    public static DPipe CURRENT_DPIPE;

    private bool starting = false;

    private BoxCollider solidCollider;
    private BoxCollider trigger;

    private void Start()
    {
        foreach (BoxCollider b in GetComponents<BoxCollider>())
        {
            if (b.isTrigger)
                trigger = b;
            else
                solidCollider = b;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")
            && !other.gameObject.GetComponent<PlayerController>().StateMachine.IsInState<Locomotion>())
            starting = true;

        if (!starting)
        {
            if (!other.gameObject.GetComponent<PlayerController>().StateMachine.IsInState<Locomotion>())
                return;
            ClimbPipe(other.GetComponent<PlayerController>());
            starting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        starting = false;
    }

    private void ClimbPipe(PlayerController player)
    {
        /*player.MoveWait(transform.position - transform.forward * 0.26f, Quaternion.LookRotation(transform.forward),
            0.4f, 16f);

        while (player.isMovingAuto)
        {
            yield return null;
        }*/

        player.Anim.applyRootMotion = false;

        //player.transform.position = transform.position - transform.forward * 0.26f;
        //player.transform.rotation = Quaternion.LookRotation(transform.forward);

        CURRENT_DPIPE = this;

        player.StateMachine.GoToState<Drainpipe>();
    }

    public BoxCollider SolidCollider
    {
        get { return solidCollider; }
    }

    public BoxCollider Trigger
    {
        get { return trigger; }
    }
}
