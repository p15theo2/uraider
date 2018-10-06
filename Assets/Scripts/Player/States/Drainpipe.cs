using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Drainpipe : StateBase<PlayerController>
{
    private bool goingToClimb = false;

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isDPipe", true);
        player.Anim.applyRootMotion = true;
        goingToClimb = false;
        player.DisableCharControl();
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isDPipe", false);
    }

    public override void Update(PlayerController player)
    {
        if (goingToClimb && player.Anim.GetCurrentAnimatorStateInfo(0).IsName("HangLoop"))
        {
            player.StateMachine.GoToState<Climbing>();
            return;
        }

        float forward = Input.GetAxisRaw("Vertical");
        float right = Input.GetAxisRaw("Horizontal");

        if (player.transform.position.y > 
            (DPipe.CURRENT_DPIPE.transform.position.y 
            + DPipe.CURRENT_DPIPE.SolidCollider.size.y - 2.2f))
        {
            bool noRootMotion = player.Anim.GetCurrentAnimatorStateInfo(0).IsName("DPipeUp")
                || player.Anim.GetCurrentAnimatorStateInfo(0).IsName("DPipeUpStop")
                || player.Anim.GetCurrentAnimatorStateInfo(0).IsName("DPipeUpStopM");
            player.Anim.applyRootMotion = !noRootMotion;

            forward = Mathf.Clamp(forward, -1f, 0f);
        }

        if (right > 0.1f)
        {
            if (ledgeDetector.FindLedgeAtPoint(player.transform.position + Vector3.up * 2.2f + player.transform.right * 1f,
                player.transform.forward, 0.2f, 0.2f))
            {
                player.Anim.SetTrigger("OffRight");
                goingToClimb = true;
                return;
            }
        }

        player.Anim.SetFloat("Forward", forward);
        player.Anim.SetFloat("Right", right);

        /*player.transform.position = new Vector3(DPipe.CURRENT_DPIPE.transform.position.x - (player.transform.forward.x * 0.28f),
            player.transform.position.y,
            DPipe.CURRENT_DPIPE.transform.position.z - (player.transform.forward.z * 0.28f));*/
    }
}
