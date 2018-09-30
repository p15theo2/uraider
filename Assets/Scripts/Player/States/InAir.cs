using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAir : StateBase<PlayerController>
{
    private bool haltUpdate = false;

    public override void OnEnter(PlayerController player)
    {
        Debug.Log("In Air");
        player.Anim.applyRootMotion = false;
        haltUpdate = false;
        //player.Velocity = Vector3.Scale(player.Velocity, new Vector3(1f, 0f, 1f));
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

    public override void HandleMessage(PlayerController player, ControllerColliderHit msg)
    {
        /*if (Mathf.Abs(Vector3.Dot(Vector3.up, msg.normal)) <= 0.05f)
        {
            Debug.Log("OMG OUCH!");
            if (player.Velocity.y > 0f)
                player.Velocity = Vector3.zero;
            else
                player.Velocity.Scale(new Vector3(0f, 1f, 0f));
        }*/
    }

    public override void Update(PlayerController player)
    {
        if (haltUpdate)
            return;

        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        player.ApplyGravity(player.gravity);

        player.Anim.SetFloat("YSpeed", player.Velocity.y);
        float targetSpeed = UMath.GetHorizontalMag(player.TargetMovementVector(player.runSpeed));
        player.Anim.SetFloat("TargetSpeed", targetSpeed);

        if (player.Grounded)
        {
            if (player.Velocity.y < -16f)
                player.StateMachine.GoToState<Dead>();
            else if (UMath.GroundAngle(player.GroundHit.normal) <= player.charControl.slopeLimit)
            {
                // Stops player moving forward on landing
                if (Input.GetAxisRaw("Vertical") < 0.1f && Input.GetAxisRaw("Horizontal") < 0.1f)
                    player.Velocity = Vector3.down * player.gravity;
                
                player.StateMachine.GoToState<Locomotion>();
                return;
            }
            else
            {
                player.StateMachine.GoToState<Sliding>();
                return;
            }
                
        } 
        else if (Input.GetButtonDown("Action") && !player.Anim.GetBool("isDive"))
        {
            player.StateMachine.GoToState<Grabbing>();
            return;
        }
    }
}
