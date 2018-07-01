using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeySwing : StateBase<PlayerController>
{
    public override void OnEnter(PlayerController player)
    {
        player.Anim.applyRootMotion = true;
        player.Anim.SetBool("isMonkey", true);
        player.MinimizeCollider();
        player.DisableCharControl();
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isMonkey", false);
        player.MaximizeCollider();
        player.EnableCharControl();
    }

    public override void Update(PlayerController player)
    {
        if (Input.GetButtonDown("Crouch"))
        {
            player.StateMachine.GoToState<InAir>();
            return;
        }

        float moveSpeed = player.walkSpeed;

        player.MoveGrounded(moveSpeed);
        player.RotateToVelocityGround(4f);
    }
}
