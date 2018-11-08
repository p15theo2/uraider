using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : StateBase<PlayerController>
{
    public override void OnEnter(PlayerController player)
    {
        player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isSliding", true);
        player.IsFootIK = true;
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isSliding", false);
        player.IsFootIK = false;
    }

    public override void Update(PlayerController player)
    {
        if (Input.GetButtonDown("Jump"))
        {
            player.RotateToVelocityGround(); // Stops player doing side jumps
            player.StateMachine.GoToState<Jumping>();
            return;
        }
        else if (!player.Grounded)
        {
            player.Velocity.Scale(new Vector3(1f, 0f, 1f));
            player.StateMachine.GoToState<InAir>();
            return;
        }
        else if (player.GroundAngle < player.charControl.slopeLimit)
        {
            player.StateMachine.GoToState<Locomotion>();
            return;
        }

        Vector3 slopeRight = Vector3.Cross(Vector3.up, player.GroundHit.normal);
        Vector3 slopeDirection = Vector3.Cross(slopeRight, player.GroundHit.normal).normalized;

        player.Velocity = slopeDirection * player.slideSpeed;
        player.Velocity.Scale(new Vector3(1f, 0f, 1f));
        player.Velocity += Vector3.down * player.gravity;

        player.RotateToVelocityGround(10f);
    }
}
