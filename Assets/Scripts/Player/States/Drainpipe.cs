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
        player.EnableCharControl();
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        if (goingToClimb)
        {
            player.Anim.applyRootMotion = true;
            if (player.Anim.GetCurrentAnimatorStateInfo(0).IsName("HangLoop"))
                player.StateMachine.GoToState<Climbing>();
            return;
        }

        if (Input.GetKeyDown(player.playerInput.crouch) && animState.IsName("DPipeIdle"))
        {
            //player.transform.position = player.transform.position - player.transform.forward * 0.2f;
            player.Velocity = Vector3.zero;
            player.StateMachine.GoToState<InAir>();
            return;
        }

        float forward = Input.GetAxisRaw(player.playerInput.verticalAxis);
        float right = Input.GetAxisRaw(player.playerInput.horizontalAxis);

        if (player.transform.position.y > 
            (DPipe.CURRENT_DPIPE.transform.position.y 
            + DPipe.CURRENT_DPIPE.SolidCollider.size.y - 2.24f))
        {
            bool noRootMotion = player.Anim.GetCurrentAnimatorStateInfo(0).IsName("DPipeUp")
                || player.Anim.GetCurrentAnimatorStateInfo(0).IsName("DPipeUpStop")
                || player.Anim.GetCurrentAnimatorStateInfo(0).IsName("DPipeUpStopM");
            player.Anim.applyRootMotion = !noRootMotion;

            forward = Mathf.Clamp(forward, -1f, 0f);
        }
        else if (player.transform.position.y < DPipe.CURRENT_DPIPE.transform.position.y + 1f)
        {
            forward = Mathf.Clamp01(forward);
        }

        if (right > 0.1f && animState.IsName("DPipeIdle"))
        {
            if (ledgeDetector.FindLedgeAtPoint(player.transform.position + Vector3.up * 2.24f + player.transform.right * 1f,
                player.transform.forward, 0.2f, 0.2f))
            {
                player.Anim.SetTrigger("OffRight");
                goingToClimb = true;
                return;
            }
        }

        player.Anim.SetFloat("Forward", forward);
        player.Anim.SetFloat("Right", right);

        player.transform.position = Vector3.Lerp(player.transform.position,
            new Vector3(DPipe.CURRENT_DPIPE.transform.position.x - (player.transform.forward.x * 0.26f),
            player.transform.position.y,
            DPipe.CURRENT_DPIPE.transform.position.z - (player.transform.forward.z * 0.26f)),
            5f * Time.deltaTime);
        player.transform.rotation = Quaternion.Lerp(player.transform.rotation, 
            Quaternion.LookRotation(DPipe.CURRENT_DPIPE.transform.forward), 5f * Time.deltaTime);
    }
}
