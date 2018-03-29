using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumping : PlayerStateBase<Jumping>
{
    private const float GRAB_TIME = 0.8f;

    private Vector3 velocity;
    private Vector3 grabPoint;
    private bool hasJumped = false;
    private bool ledgesDetected = false;
    private float timeTracker = 0.0f;
    private float grabForwardOffset = 0.1f;
    private float grabUpOffset = 2.1f; //1.78

    private LedgeDetector ledgeDetector = new LedgeDetector();

    public override void OnEnter(PlayerController player)
    {
        player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isJumping", true);

        velocity = player.Velocity;
        velocity.y = 0f;

        if (ledgeDetector.FindLedgeJump(player.transform.position,
            player.transform.forward, 4.4f, 3.4f))
        {
            if (!Physics.Raycast(ledgeDetector.GrabPoint, Vector3.down, 2.0f))
                ledgesDetected = true;
        }

        player.Anim.SetBool("isGrabbing", ledgesDetected);
    }

    public override void OnExit(PlayerController player)
    {
        hasJumped = false;
        ledgesDetected = false;
        player.Anim.SetBool("isJumping", false);
        player.Anim.SetBool("isGrabbing", false);
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        if ((animState.IsName("RunJump") || animState.IsName("JumpUp")) && !hasJumped)
        {
            float curSpeed = UMath.GetHorizontalMag(player.Velocity);

            if (!ledgesDetected)
            {
                float zVel = curSpeed > 1.2f ? player.jumpZVel
                    : curSpeed > 0.1f ? player.sJumpZVel
                    : 0.1f;
                float yVel = curSpeed > 1.2f ? player.jumpYVel
                    : player.jumpYVel;

                velocity = player.transform.forward * zVel
                    + Vector3.up * yVel;
            }
            else
            {
                grabPoint = new Vector3(ledgeDetector.GrabPoint.x - (player.transform.forward.x * grabForwardOffset), 
                    ledgeDetector.GrabPoint.y - grabUpOffset, 
                    ledgeDetector.GrabPoint.z - (player.transform.forward.z * grabForwardOffset));

                velocity = UMath.VelocityToReachPoint(player.transform.position,
                    grabPoint,
                    player.gravity,
                    GRAB_TIME);

                timeTracker = Time.time;
            }

            hasJumped = true;
        }
        else if (hasJumped)
        {
            velocity.y -= player.gravity * Time.deltaTime;

            if (ledgesDetected && Time.time - timeTracker >= GRAB_TIME)
            {
                player.transform.position = grabPoint;
                player.State = Climbing.Instance;
            }
            else if (player.Grounded)
            {
                player.State = Locomotion.Instance;
            }
        }

        player.Velocity = velocity;
    }
}
