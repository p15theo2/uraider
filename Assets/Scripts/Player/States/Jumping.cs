using System.Collections;
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
        //player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isJumping", true);
        lastChance = Random.Range(0f, 1f);
        player.Anim.SetFloat("lastChance", lastChance);
        player.Velocity = Vector3.Scale(player.Velocity, new Vector3(1f, 0f, 1f));

        if (player.autoLedgeTarget)
        {
            ledgesDetected = ledgeDetector.FindLedgeJump(player.transform.position,
                player.transform.forward, 5.6f, 3.4f);
        }
        else
        {
            ledgesDetected = false;
        }
    }

    public override void OnExit(PlayerController player)
    {
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
        player.Anim.SetFloat("TargetSpeed", UMath.GetHorizontalMag(player.TargetMovementVector(player.runSpeed)));

        if (!player.autoLedgeTarget && Input.GetButton("Action"))
        {
            isGrabbing = true;
        }

        if (transInfo.IsName("Locomotion -> Stand_Compress"))
        {
            player.Velocity = Vector3.zero;
        }

        if ((animState.IsName("RunJump") || animState.IsName("JumpUp") || animState.IsName("SprintJump")) && !hasJumped)
        {
            player.Anim.applyRootMotion = false;
            float curSpeed = UMath.GetHorizontalMag(player.Velocity);

            if (ledgesDetected)
            {
                grabType = ledgeDetector.GetGrabType(player.transform.position, player.transform.forward,
                player.jumpZBoost, player.jumpYVel, -player.gravity);

                if (grabType == GrabType.Hand || ledgeDetector.WallType == LedgeType.Free)
                {
                    grabPoint = new Vector3(ledgeDetector.GrabPoint.x - (player.transform.forward.x * grabForwardOffset),
                        ledgeDetector.GrabPoint.y - grabUpOffset,
                        ledgeDetector.GrabPoint.z - (player.transform.forward.z * grabForwardOffset));

                    player.Velocity = UMath.VelocityToReachPoint(player.transform.position,
                        grabPoint,
                        player.gravity,
                        GRAB_TIME);

                    timeTracker = Time.time;

                    player.Anim.SetBool("isGrabbing", true);
                }
                else
                {
                    ledgesDetected = false;
                }
            }

            if (!ledgesDetected)  // can change in previous if - so NO else if
            {
                /*float zVel = curSpeed > player.walkSpeed ? player.jumpZVel
                    : curSpeed > 0.1f ? player.sJumpZVel
                    : 0.1f;
                float yVel = curSpeed > player.walkSpeed ? player.jumpYVel
                    : player.jumpYVel;*/

                float zVel = curSpeed > 0.1f ? curSpeed + player.jumpZBoost
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

            if (ledgesDetected && Time.time - timeTracker >= GRAB_TIME)
            {
                player.transform.position = grabPoint;

                if (ledgeDetector.WallType == LedgeType.Free)
                    player.StateMachine.GoToState<Freeclimb>();
                else if (grabType == GrabType.Hand)
                    player.StateMachine.GoToState<Climbing>();
                else
                    player.StateMachine.GoToState<Locomotion>();
            }
            else if (player.Grounded && player.groundDistance < 0.1f && !ledgesDetected)
            {
                player.StateMachine.GoToState<Locomotion>();
            }
            else if (isGrabbing)
            {
                /*if (player.Velocity.y < -10f)
                {
                    isGrabbing = false;
                    return;
                }

                if (ledgeDetector.FindLedgeAtPoint(player.transform.position + Vector3.up * grabUpOffset,
                    player.transform.forward,
                    0.3f,
                    0.2f))
                {
                    grabPoint = new Vector3(ledgeDetector.GrabPoint.x - (player.transform.forward.x * grabForwardOffset),
                        ledgeDetector.GrabPoint.y - grabUpOffset,
                        ledgeDetector.GrabPoint.z - (player.transform.forward.z * grabForwardOffset));

                    grabType = ledgeDetector.GetGrabType(player.transform.position, player.transform.forward,
                        player.jumpZVel, player.jumpYVel, -player.gravity);

                    player.transform.position = grabPoint;

                    if (ledgeDetector.WallType == LedgeType.Free)
                        player.StateMachine.GoToState<Freeclimb>();
                    else if (grabType == GrabType.Hand)
                        player.StateMachine.GoToState<Climbing>();
                    else
                        player.StateMachine.GoToState<Locomotion>();
                }*/
            }
        }
    }
}
