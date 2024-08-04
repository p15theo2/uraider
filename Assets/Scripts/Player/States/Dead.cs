using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dead : StateBase<PlayerController>
{
    private bool ragged = false;
    private float timeCounter = 0f;

    private Vector3 hitVelocity;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isDead", true);
        player.Anim.applyRootMotion = true;
        hitVelocity = player.Velocity;
        player.Velocity = Vector3.zero;
        player.camController.PivotOnTarget();
        //player.Anim.enabled = false;
        //player.EnableRagdoll();
        timeCounter = Time.time;
        if(!player.Anim.GetCurrentAnimatorStateInfo(0).IsName("SwimSubmerged") && !player.Anim.GetCurrentAnimatorStateInfo(0).IsName("TreadSubmerged"))
        foreach (Rigidbody rb in player.ragRigidBodies)
        {
            rb.velocity = player.Velocity;
        }
        player.camController.target = player.ragRigidBodies[0].transform;
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isDead", false);
    }

    public override void Update(PlayerController player)
    {
        if (Time.time - timeCounter >= 7f && !ragged)
        {
            ragged = true;
            player.Anim.enabled = false;
            player.EnableRagdoll();
            player.Velocity = Vector3.Scale(hitVelocity.normalized, new Vector3(1f, -1f, 1f));
            foreach (Rigidbody rb in player.ragRigidBodies)
            {
                rb.velocity = player.Velocity;
            }
        }

        if (Time.time - timeCounter >= 7f)
        {
            SceneManager.LoadScene("DevArea");
        }
    }
}
