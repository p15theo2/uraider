using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Drainpipe : StateBase<PlayerController>
{
    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isDPipe", true);
        player.Anim.applyRootMotion = true;
        player.DisableCharControl();
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isDPipe", false);
    }

    public override void Update(PlayerController player)
    {
        player.Anim.SetFloat("Forward", Input.GetAxisRaw("Vertical"));
        player.Anim.SetFloat("Right", Input.GetAxisRaw("Horizontal"));

        /*player.transform.position = new Vector3(DPipe.CURRENT_DPIPE.transform.position.x - (player.transform.forward.x * 0.3f),
            player.transform.position.y,
            DPipe.CURRENT_DPIPE.transform.position.z - (player.transform.forward.z * 0.3f));*/
    }
}
