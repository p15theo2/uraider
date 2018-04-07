using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dead : StateBase<PlayerController>
{
    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isDead", true);
        player.Anim.applyRootMotion = true;
        player.Velocity = Vector3.zero;
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isDead", false);
    }

    public override void Update(PlayerController player)
    {
        
    }
}
