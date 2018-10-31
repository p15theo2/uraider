using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : StateBase<PlayerController>
{
    private bool isTransitioning = false;
    private bool isIdle = false;

    private LadderVolume currentLadder;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isLadder", true);
        player.Anim.applyRootMotion = true;
        player.DisableCharControl();
        isTransitioning = false;
        isIdle = false;
        currentLadder = LadderVolume.CURRENT_LADDER;
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isLadder", false);
        player.Anim.SetFloat("Speed", 0f);
        player.EnableCharControl();
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        if (isTransitioning)
        {
            player.Velocity = Vector3.zero;

            if (isIdle)
                player.StateMachine.GoToState<Locomotion>();

            if (animState.IsName("Idle"))
            {
                player.EnableCharControl();
                isIdle = true;
            }

            return;
        }

        Vector3 ladderAdjusted = currentLadder.transform.position - currentLadder.transform.forward * 0.4f;
        player.transform.position = Vector3.Lerp(player.transform.position, 
            new Vector3(ladderAdjusted.x, player.transform.position.y, ladderAdjusted.z), 5f * Time.deltaTime);
        player.transform.rotation = Quaternion.Lerp(player.transform.rotation, 
            Quaternion.LookRotation(currentLadder.transform.forward), 5f * Time.deltaTime);

        float forward = Input.GetAxisRaw(player.playerInput.verticalAxis);
        float right = Input.GetAxisRaw(player.playerInput.horizontalAxis);

        if (Input.GetKeyDown(player.playerInput.crouch) && animState.IsName("LadderIdle"))
        {
            player.Velocity = Vector3.zero;
            player.StateMachine.GoToState<InAir>();
            return;
        }

        if (player.transform.position.y > currentLadder.transform.position.y
            + (currentLadder.MainCollider.size.y - player.charControl.height))
        {
            forward = Mathf.Clamp(forward, -1f, 0f);
        }
        else if (player.transform.position.y < currentLadder.transform.position.y + 1f)
        {
            forward = Mathf.Clamp01(forward);
        }

        player.Anim.SetFloat("Forward", forward);
        player.Anim.SetFloat("Right", right);

        if (animState.IsName("LadderIdle"))
        {
            if (right < -0.1f)
            {
                if (CheckForLedge(player.transform.position - (player.transform.right * 0.6f)))
                {
                    player.Anim.SetTrigger("OffLeft");
                    isTransitioning = true;
                }
            }
            else if (right > 0.1f)
            {
                if (CheckForLedge(player.transform.position + (player.transform.right * 0.6f)))
                {
                    player.Anim.SetTrigger("OffRight");
                    isTransitioning = true;
                }
            }
        }
    }

    public bool CheckForLedge(Vector3 start)
    {
        return Physics.Raycast(start + Vector3.up * 0.5f, Vector3.down, 1f);
    }
}
