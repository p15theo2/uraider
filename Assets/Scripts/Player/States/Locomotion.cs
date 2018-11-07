using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : StateBase<PlayerController>
{
    private bool isRootMotion = false;  // Used for root motion of step ups
    private bool waitingBool = false;  // avoids early reset of root mtn
    private bool isJustEntered = false;
    private bool isTransitioning = false;
    private bool isStairs = false;
    private bool isJumping = false;

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.camController.LAUTurning = true;
        player.EnableCharControl();
        player.Anim.SetBool("isJumping", false);
        player.Anim.SetBool("isLocomotion", true);
        player.Anim.applyRootMotion = true;
        player.IsFootIK = true;
        isTransitioning = false;
        isJumping = false;
        isRootMotion = false;
    }

    public override void OnExit(PlayerController player)
    {
        player.camController.LAUTurning = false;
        player.Anim.SetBool("isLocomotion", false);
        player.IsFootIK = false;
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorTransitionInfo transInfo = player.Anim.GetAnimatorTransitionInfo(0);

        player.IsFootIK = UMath.GetHorizontalMag(player.Velocity) < 0.1f;

        if (player.isMovingAuto)
            return;

        if (isJumping && !player.adjustingRot)
        {
            player.StateMachine.GoToState<Jumping>();
            return;
        }

        if (isTransitioning)
        {
            if (animState.IsName("HangLoop"))
            {
                player.Anim.ResetTrigger("ToLedgeForward");
                player.StateMachine.GoToState<Climbing>();
            }
            else if (animState.IsName("LedgeOffFront"))
            {
                player.Anim.MatchTarget(new Vector3(
                ledgeDetector.GrabPoint.x
                - (ledgeDetector.Direction.x * player.hangForwardOffset),

                ledgeDetector.GrabPoint.y - player.hangUpOffset,

                ledgeDetector.GrabPoint.z
                - (ledgeDetector.Direction.x * player.hangForwardOffset)
                ),
                Quaternion.LookRotation(ledgeDetector.Direction, Vector3.up),
                AvatarTarget.Root,
                new MatchTargetWeightMask(Vector3.one, 1f),
                0.2f, 1f);
            }
            return;
        }
        else if (!player.Grounded && player.groundDistance > player.charControl.stepOffset && !isRootMotion)
        {
            player.Velocity = Vector3.Scale(player.Velocity, new Vector3(1f, 0f, 1f));
            player.StateMachine.GoToState<InAir>();
            return;
        }

        if (isStairs = (player.groundDistance < 1f && player.GroundHit.collider.CompareTag("Stairs")))
        {
            player.Anim.SetBool("isStairs", true);
            RaycastHit hit;
            if (Physics.Raycast(player.transform.position + player.transform.forward * 0.2f + 0.2f * Vector3.up,
                Vector3.down, out hit, 1f))
            {
                player.Anim.SetFloat("Stairs", hit.point.y < player.transform.position.y ? -1f : 1f, 0.1f, Time.deltaTime);
                Debug.Log(player.transform.position.y - hit.point.y);
            }
        }
        else
        {
            player.Anim.SetBool("isStairs", false);
            player.Anim.SetFloat("Stairs", 0f, 0.1f, Time.deltaTime);
        }

        if (player.groundAngle > player.charControl.slopeLimit && !isRootMotion)
        {
            player.StateMachine.GoToState<Sliding>();
            return;
        }
        else if (Input.GetKeyDown(player.playerInput.drawWeapon) || Input.GetAxisRaw("CombatTrigger") > 0.1f)
        {
            player.StateMachine.GoToState<Combat>();
            return;
        }

        float moveSpeed = Input.GetKey(player.playerInput.walk) ? player.walkSpeed
            : player.runSpeed;

        player.MoveGrounded(moveSpeed);
        if (player.targetSpeed > 1f)
            player.RotateToVelocityGround();
        HandleLedgeStepMotion(player);
        LookForStepLedges(player);

        if (Input.GetKeyDown(player.playerInput.crouch))
        {
            Vector3 start = player.transform.position
                + player.transform.forward * 0.5f
                + Vector3.down * 0.1f;
            if (ledgeDetector.FindLedgeAtPoint(start, -player.transform.forward, 0.5f, 0.2f))
            {
                isTransitioning = true;
                player.Anim.SetTrigger("ToLedgeForward");
                player.Anim.applyRootMotion = true;
                player.DisableCharControl();
                return;
            }
            else
            {
                if (UMath.GetHorizontalMag(player.Velocity) < 2f)
                    player.StateMachine.GoToState<Crouch>();
                else
                {
                    player.Anim.SetTrigger("Slide");
                    player.charControl.height = 0.5f;
                    player.charControl.center = Vector3.up * 0.25f;
                }

            }
        }

        if (Input.GetKeyDown(player.playerInput.jump) && !isRootMotion)
            isJumping = true;
    }

    private void LookForStepLedges(PlayerController player)
    {
        if (Input.GetButtonDown("Jump") && !isRootMotion && player.Anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            isRootMotion = ledgeDetector.FindPlatformInfront(player.transform.position,
                player.transform.forward, 2f);

            if (isRootMotion)
            {
                float height = ledgeDetector.GrabPoint.y - player.transform.position.y;

                // step can be runned over
                if (height < player.charControl.stepOffset)
                {
                    isRootMotion = false;
                    return;
                } 
                else
                {
                    player.transform.rotation = Quaternion.LookRotation(ledgeDetector.Direction, Vector3.up);
                    player.DisableCharControl(); // stops char controller collisions
                }

                if (height <= 0.9f)
                    player.Anim.SetTrigger("StepUpQtr");
                else if (height <= 1.5f)
                    player.Anim.SetTrigger("StepUpHlf");
                else
                    player.Anim.SetTrigger("StepUpFull");

                waitingBool = true;
            }
        }
    }

    private void HandleLedgeStepMotion(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        if (!waitingBool && isRootMotion && animState.IsName("Idle"))
        {
            player.EnableCharControl();
            isRootMotion = false;
        }
        else if (waitingBool && (animState.IsName("StepUp_Hlf") || animState.IsName("StepUp_Qtr")
            || animState.IsName("StepUp_Full") || animState.IsName("RunStepUp_Qtr") || animState.IsName("RunStepUp_QtrM")))
        {
            waitingBool = false;
        }
    }
}
