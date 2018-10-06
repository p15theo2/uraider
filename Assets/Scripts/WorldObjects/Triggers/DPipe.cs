using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DPipe : MonoBehaviour
{
    public static DPipe CURRENT_DPIPE;

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
        if (Input.GetButtonDown("Action"))
        {
            StartCoroutine(ClimbPipe(other.GetComponent<PlayerController>()));
        }
    }

    private IEnumerator ClimbPipe(PlayerController player)
    {
        player.MoveWait(transform.position - transform.forward * 0.26f, Quaternion.LookRotation(transform.forward),
            0.4f, 16f);

        while (player.isMovingAuto)
        {
            yield return null;
        }

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
