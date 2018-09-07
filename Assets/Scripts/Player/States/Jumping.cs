﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumping : StateBase<PlayerController>
{
    private const float GRAB_TIME = 0.8f;

    private Vector3 grabPoint;
    private GrabType grabType;
    private bool hasJumped = false;
    private bool ledgesDetected = false;
    private bool isGrabbing = false;
    private float timeTracker = 0.0f;
    private float grabForwardOffset = 0.1f;
    private float grabUpOffset = 2.1f; //1.78
    private float hipForwardOffset = 0.2f;
    private float hipUpOffset = 0.94f;
    private float lastChance = 0f;

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isJumping", true);
        //player.Anim.applyRootMotion = true;
        lastChance = Random.Range(0f, 1f);
        player.Anim.SetFloat("lastChance", lastChance);
        player.Velocity = Vector3.Scale(player.Velocity, new Vector3(1f, 0f, 1f));

        if (player.autoLedgeTarget)
        {
            ledgesDetected = ledgeDetector.FindLedgeJump(player.transform.position,
                player.transform.forward, 5.6f, 3.4f);
        }
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isDive", false);
        hasJumped = false;
        isGrabbing = false;
        ledgesDetected = false;
        player.Anim.SetBool("isJumping", false);
        player.Anim.SetBool("isGrabbing", false);
        if (player.Velocity.y < -10f && player.Grounded)
            player.Stats.Health += (int)player.Velocity.y;
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorTransitionInfo transInfo = player.Anim.GetAnimatorTransitionInfo(0);

        player.Anim.SetFloat("YSpeed", player.Velocity.y);
        float targetSpeed = UMath.GetHorizontalMag(player.TargetMovementVector(player.runSpeed));
        player.Anim.SetFloat("TargetSpeed", targetSpeed);

        if (Input.GetButtonDown("Sprint"))
        {
            player.Anim.SetBool("isDive", true);
        }

        if (!player.autoLedgeTarget && Input.GetButton("Action"))
        {
            isGrabbing = true;
        }

        if (transInfo.IsName("Locomotion -> Stand_Compress"))
        {
            player.Velocity = Vector3.zero;
        }

        if ((animState.IsName("RunJump") || animState.IsName("JumpUp") 
            || animState.IsName("SprintJump") || animState.IsName("Dive")
            || animState.IsName("StandJump")) && !hasJumped)
        {
            player.Anim.applyRootMotion = false;
            float curSpeed = UMath.GetHorizontalMag(player.Velocity);

            if (ledgesDetected)
            {
                grabType = ledgeDetector.GetGrabType(player.transform.position, player.transform.forward,
                player.jumpZBoost, player.jumpYVel, -player.gravity);

                if (grabType == GrabType.Hand || ledgeDetector.WallType == LedgeType.Free)
                {
                    player.StateMachine.GoToState<AutoGrabbing>();
                    return;
                }
                else
                {
                    ledgesDetected = false;
                }
            }

            if (!ledgesDetected)  // can change in previous if - so NO else if
            {
                float zVel = curSpeed > 0.1f ? curSpeed + player.jumpZBoost
                    : targetSpeed > 0.1f ? 3f
                    : 0.1f;
                float yVel = player.jumpYVel;

                player.Velocity = player.transform.forward * zVel
                    + Vector3.up * yVel;
            }

            hasJumped = true;
        }
        else if (hasJumped)
        {
            if (isGrabbing)
            {
                player.StateMachine.GoToState<Grabbing>();
                return;
            }

            player.ApplyGravity(player.gravity);

            if (player.Grounded)
                player.StateMachine.GoToState<Locomotion>();
        }
    }
}
