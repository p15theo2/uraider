using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : StateBase<PlayerController>
{
    private bool isTransitioning = false;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isLadder", true);
        player.Anim.applyRootMotion = true;
        player.DisableCharControl();
        isTransitioning = false;
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isLadder", false);
        player.EnableCharControl();
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        if (isTransitioning)
        {
            if (animState.IsName("Idle"))
                player.StateMachine.GoToState<Locomotion>();

            return;
        }

        player.Anim.SetFloat("Forward", Input.GetAxisRaw("Vertical"));
        player.Anim.SetFloat("Right", Input.GetAxisRaw("Horizontal"));

        if (Input.GetAxisRaw("Horizontal") < -0.1f)
        {
            if (CheckForLedge(player.transform.position - (player.transform.right * 0.6f))) 
            {
                player.Anim.SetTrigger("OffLeft");
                isTransitioning = true;
            }
        }
        else if (Input.GetAxisRaw("Horizontal") > 0.1f)
        {
            if (CheckForLedge(player.transform.position + (player.transform.right * 0.6f)))
            {
                player.Anim.SetTrigger("OffRight");
                isTransitioning = true;
            }
        }
    }

    public bool CheckForLedge(Vector3 start)
    {
        return Physics.Raycast(start, Vector3.down, 0.6f);
    }
}
