using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAir : StateBase<PlayerController>
{
    private bool haltUpdate = false;
    private bool screamed = false;

    public override void OnEnter(PlayerController player)
    {
        
        player.Anim.applyRootMotion = false;
        haltUpdate = false;
        screamed = false;
    }

    public override void OnExit(PlayerController player)
    {
        haltUpdate = false;

        if (player.Velocity.y < -10f && player.Grounded)
            player.Stats.Health += (int)player.Velocity.y;

        player.Anim.SetBool("isJumping", false);
        player.Anim.SetBool("isGrabbing", false);
        player.Anim.SetBool("isDive", false);
    }

    public override void Update(PlayerController player)
    {
        Debug.Log("In Air");

        if (haltUpdate)
            return;

        if (player.Velocity.y < -player.deathVelocity && !screamed)
        {
            player.SFX.PlayScreamSound();
            screamed = true;
        }

        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        player.ApplyGravity(player.gravity);

        player.Anim.SetFloat("YSpeed", player.Velocity.y);
        float targetSpeed = UMath.GetHorizontalMag(player.RawTargetVector() * player.runSpeed);
        player.Anim.SetFloat("TargetSpeed", targetSpeed);

        if (player.Grounded)
        {
            if (player.Velocity.y < -player.deathVelocity)
            {
                player.GetComponent<AudioSource>().Stop();
                player.SFX.PlayHitGroundSound();
                player.StateMachine.GoToState<Dead>();
            }
            else if (UMath.GroundAngle(player.GroundHit.normal) <= player.charControl.slopeLimit)
            {
                // Stops player moving forward on landing
                if (Input.GetAxisRaw(player.playerInput.verticalAxis) < 0.1f && Input.GetAxisRaw(player.playerInput.horizontalAxis) < 0.1f)
                    player.Velocity = Vector3.down * player.gravity;
                
                player.StateMachine.GoToState<Locomotion>();
            }
            else
            {
                player.StateMachine.GoToState<Sliding>();
            }
            return;
                
        } 
        else if (Input.GetKeyDown(player.playerInput.action) && !player.Anim.GetBool("isDive"))
        {
            player.StateMachine.GoToState<Grabbing>();
            return;
        }
    }
}
