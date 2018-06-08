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
    private float timeTracker = 0.0f;
    private float grabForwardOffset = 0.1f;
    private float grabUpOffset = 2.1f; //1.78
    private float hipForwardOffset = 0.2f;
    private float hipUpOffset = 0.94f;

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        //player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isJumping", true);

        player.Velocity = Vector3.Scale(player.Velocity, new Vector3(1f, 0f, 1f));

        ledgesDetected = ledgeDetector.FindLedgeJump(player.transform.position,
            player.transform.forward, 5.6f, 3.4f);
    }

    public override void OnExit(PlayerController player)
    {
        hasJumped = false;
        ledgesDetected = false;
        player.Anim.SetBool("isJumping", false);
        player.Anim.SetBool("isGrabbing", false);
        if (player.Velocity.y < -10f && player.Grounded)
            player.Stats.Health += (int)player.Velocity.y;
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        if ((animState.IsName("RunJump") || animState.IsName("JumpUp")) && !hasJumped)
        {
            player.Anim.applyRootMotion = false;
            float curSpeed = UMath.GetHorizontalMag(player.Velocity);

            if (ledgesDetected)
            {
                grabType = ledgeDetector.GetGrabType(player.transform.position, player.transform.forward,
                player.jumpZVel, player.jumpYVel, -player.gravity);

                if (grabType == GrabType.Hand || ledgeDetector.WallType == LedgeType.Free)
                {
                    Debug.Log("Hand me");

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
                else if (grabType == GrabType.Hip)
                {
                    Debug.Log("Hip me");

                    grabPoint = new Vector3(ledgeDetector.GrabPoint.x - (player.transform.forward.x * hipForwardOffset),
                        ledgeDetector.GrabPoint.y - hipUpOffset,
                        ledgeDetector.GrabPoint.z - (player.transform.forward.z * hipForwardOffset));

                    player.Velocity = UMath.VelocityToReachPoint(player.transform.position,
                        grabPoint,
                        player.gravity,
                        GRAB_TIME);

                    timeTracker = Time.time;

                    player.Anim.SetBool("isHipping", true);
                }
                else
                {
                    ledgesDetected = false;
                }
            }

            if (!ledgesDetected)  // can change in previous if - so NO else if
            {
                float zVel = curSpeed > player.walkSpeed ? player.jumpZVel
                    : curSpeed > 0.1f ? player.sJumpZVel
                    : 0.1f;
                float yVel = curSpeed > player.walkSpeed ? player.jumpYVel
                    : player.jumpYVel;

                player.Velocity = player.transform.forward * zVel
                    + Vector3.up * yVel;
            }

            hasJumped = true;
        }
        else if (hasJumped)
        {
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
            else if (player.Grounded && !ledgesDetected)
            {
                player.StateMachine.GoToState<Locomotion>();
            }
        }
    }
}
