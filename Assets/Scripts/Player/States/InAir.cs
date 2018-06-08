using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAir : StateBase<PlayerController>
{
    public override void OnEnter(PlayerController player)
    {
        player.Anim.applyRootMotion = false;
        player.Velocity = Vector3.Scale(player.Velocity, new Vector3(1f, 0f, 1f));
    }

    public override void OnExit(PlayerController player)
    {
        
    }

    public override void Update(PlayerController player)
    {
        player.ApplyGravity(player.gravity);

        player.Anim.SetFloat("YSpeed", player.Velocity.y);

        if (player.Grounded)
            player.StateMachine.GoToState<Locomotion>();
    }
}
