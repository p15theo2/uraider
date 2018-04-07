using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crouch : StateBase<PlayerController>
{
    public override void OnEnter(PlayerController player)
    {
        player.DisableCharControl();
        player.Anim.applyRootMotion = true;
        player.Anim.SetBool("isCrouch", true);
        player.Velocity = Vector3.zero;
    }

    public override void OnExit(PlayerController player)
    {
        player.EnableCharControl();
        player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isCrouch", false);
    }

    public override void Update(PlayerController player)
    {
        if(!Input.GetButton("Crouch"))
        {
            player.StateMachine.GoToState<Locomotion>();
            return;
        }

        float moveSpeed = player.walkSpeed;

        player.MoveGrounded(moveSpeed);
        player.RotateToVelocityGround(4f);
    }
}
