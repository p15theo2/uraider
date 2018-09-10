using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : StateBase<PlayerController>
{
    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isSliding", true);
        player.IsFootIK = true;
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isSliding", false);
        player.IsFootIK = false;
    }

    public override void HandleMessage(PlayerController player, string msg)
    {
        if (msg == "STOP_SLIDE")
        {
            if (player.Grounded)
                player.StateMachine.GoToState<Locomotion>();
            else
                player.StateMachine.GoToState<InAir>();
        }
    }

    public override void Update(PlayerController player)
    {
        if (Input.GetButtonDown("Jump"))
        {
            player.RotateToVelocityGround(); // Stops player doing side jumps
            player.StateMachine.GoToState<Jumping>();
            return;
        }

        player.Velocity = player.slopeDirection * player.slideSpeed;
        player.Velocity.Scale(new Vector3(1f, 0f, 1f));
        player.Velocity += Vector3.down * player.gravity;

        player.RotateToVelocityGround(10f);
    }
}
