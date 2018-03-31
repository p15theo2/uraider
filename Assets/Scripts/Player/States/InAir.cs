using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAir : PlayerStateBase<InAir>
{
    private Vector3 velocity;

    public override void OnEnter(PlayerController player)
    {
        velocity = player.Velocity;
        velocity.y = 0f;
    }

    public override void OnExit(PlayerController player)
    {
        
    }

    public override void Update(PlayerController player)
    {
        velocity.y -= player.gravity * Time.deltaTime;
        player.Anim.SetFloat("YSpeed", velocity.y);

        player.Velocity = velocity;

        if (player.Grounded)
            player.State = Locomotion.Instance;
    }
}
