using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : StateBase<PlayerController>
{
    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isSliding", true);
        Debug.Log("yo yo im in slide");
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isSliding", false);
    }

    public override void HandleMessage(PlayerController player, string msg)
    {
        if (msg == "STOP_SLIDE")
        {
            Debug.Log("stop siding");
            if (player.Grounded)
                player.StateMachine.GoToState<Locomotion>();
            else
                player.StateMachine.GoToState<InAir>();
        }
    }

    public override void Update(PlayerController player)
    {
        player.Velocity = player.slopeDirection * player.slideSpeed;
        player.Velocity.Scale(new Vector3(1f, 0f, 1f));
        player.ApplyGravity(player.gravity);

        player.RotateToVelocityGround(10f);
    }
}
