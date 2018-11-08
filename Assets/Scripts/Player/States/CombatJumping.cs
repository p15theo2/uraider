using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatJumping : StateBase<PlayerController>
{
    private bool hasJumped = false;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isCombatJumping", true);
        hasJumped = false;
        float absAngle = Mathf.Abs(player.CombatAngle);
        player.transform.rotation = absAngle > 45f && absAngle < 135f ?
            Quaternion.LookRotation(Vector3.Cross(player.transform.forward, Vector3.up))
            : Quaternion.LookRotation((absAngle <= 45f ? 1f : -1f) * 
            Vector3.Scale(new Vector3(1f, 0f, 1f), player.Velocity.normalized));
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isCombatJumping", false);
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorTransitionInfo transInfo = player.Anim.GetAnimatorTransitionInfo(0);

        if (hasJumped)
        {
            player.ApplyGravity(player.gravity);

            if (player.Grounded || animState.normalizedTime >= 1f)
                player.StateMachine.GoToState<Combat>();
        }
        else
        {
            if (transInfo.IsName("CombatCompress -> JumpR"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.right * 4f + Vector3.up * player.jumpYVel;
                hasJumped = true;
            }
            else if (transInfo.IsName("CombatCompress -> JumpL"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.right * -4f + Vector3.up * player.jumpYVel;
                hasJumped = true;
            }
            else if (transInfo.IsName("CombatCompress -> JumpB"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.forward * -4f + Vector3.up * player.jumpYVel;
                hasJumped = true;
            }
            else if (transInfo.IsName("CombatCompress -> JumpF"))
            {
                player.Velocity = player.transform.forward * 4f + Vector3.up * player.jumpYVel;
                hasJumped = true;
            }
            
        }
    }
}
