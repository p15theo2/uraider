using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : StateBase<PlayerController>
{
    private bool isRootMotion = false;  // Used for root motion of step ups
    private bool waitingBool = false;  // avoids early reset of root mtn
    private bool isJustEntered = false;
    private bool isTransitioning = false;

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isJumping", false);
        player.Anim.applyRootMotion = false;
        isTransitioning = false;
        isRootMotion = false;
    }

    public override void OnExit(PlayerController player)
    {
        isRootMotion = false;
        isTransitioning = false;
    }

    public override void HandleMessage(PlayerController player, string msg)
    {
        /*if (msg == "SLIDE" && !isRootMotion)
        {
            player.StateMachine.GoToState<Sliding>();
            isTransitioning = true;
        }*/
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorTransitionInfo transInfo = player.Anim.GetAnimatorTransitionInfo(0);

        if (player.isMovingAuto || isTransitioning)
        {
            if (animState.IsName("HangLoop"))
            {
                player.StateMachine.GoToState<Climbing>();
            }
            return;
        }
        else if (animState.IsName("FastLand"))
        {
            player.MoveGrounded(0f);
            return;
        }
        else if (!player.Grounded && player.groundDistance > player.charControl.stepOffset && !isRootMotion)
        {
            player.StateMachine.GoToState<InAir>();
            return;
        }
        else if (player.Grounded)
        {
            if (player.groundAngle > player.charControl.slopeLimit && !isRootMotion)
            {
                player.StateMachine.GoToState<Sliding>();
                return;
            }
            else if (Input.GetButtonDown("Draw Weapon"))
            {
                player.StateMachine.GoToState<Combat>();
                return;
            }
        }

        float moveSpeed = Input.GetButton("Walk") ? player.walkSpeed
            : Input.GetButton("Sprint") ? player.sprintSpeed
            : player.runSpeed;

        player.Anim.SetFloat("Forward", Input.GetAxisRaw("Vertical"));

        if (animState.IsName("Locomotion") || animState.IsName("RunJump_to_Run") || transInfo.IsName("FallMed -> RunJump_to_Run")
            || transInfo.IsName("Turn180 -> Locomotion"))
        {
            player.MoveGrounded(moveSpeed);
            player.RotateToVelocityGround();
            player.Anim.applyRootMotion = false;
        }
        else
        {
            player.MoveGrounded(0f);
            player.Velocity = Vector3.down * player.gravity;
            player.Anim.applyRootMotion = true;
        }

        HandleLedgeStepMotion(player);
        LookForStepLedges(player);
        StopLaraFloating(player);

        if (Input.GetButtonDown("Crouch"))
        {
            Vector3 start = player.transform.position
                + player.transform.forward * 0.4f
                + Vector3.down * 0.1f;
            if (ledgeDetector.FindLedgeAtPoint(start, -player.transform.forward, 0.5f, 0.2f))
            {
                player.DisableCharControl();
                player.Anim.SetTrigger("ToLedgeForward");
                player.Anim.applyRootMotion = true;
                player.MoveWait(ledgeDetector.GrabPoint - player.transform.forward * 0.2f,
                    Quaternion.LookRotation(-ledgeDetector.Direction,Vector3.up));
                isTransitioning = true;
                return;
            }
            else
            {
                player.StateMachine.GoToState<Crouch>();
            }
        }

        if (Input.GetButtonDown("Jump") && !isRootMotion)
            player.StateMachine.GoToState<Jumping>();
    }

    private void StopLaraFloating(PlayerController player)
    {
        Vector3 centerStart = player.transform.position + Vector3.up * 0.2f;

        List<Vector3> sideChecks = new List<Vector3>();
        sideChecks.Add(centerStart + player.transform.forward * player.charControl.radius);
        //sideChecks.Add(centerStart - player.transform.forward * player.charControl.radius);
        sideChecks.Add(centerStart + player.transform.right * player.charControl.radius);
        sideChecks.Add(centerStart - player.transform.right * player.charControl.radius);

        bool push = true;

        RaycastHit hit = new RaycastHit();
        foreach (Vector3 v in sideChecks)
        {
            if (Physics.Raycast(v, Vector3.down, out hit, 2f))
                push = false;
        }

        // Lara is floating off edge basically
        if (/*hitCount == 1*/push && !player.Grounded)
            player.Velocity = Mathf.Max(player.walkSpeed, UMath.GetHorizontalMag(player.Velocity)) * player.transform.forward;  // push Lara
    }

    private void LookForStepLedges(PlayerController player)
    {
        if (Input.GetButtonDown("Jump") && !isRootMotion)
        {
            isRootMotion = ledgeDetector.FindPlatformInfront(player.transform.position,
                player.transform.forward, 2f);

            if (isRootMotion)
            {
                player.Anim.applyRootMotion = true;
                float height = ledgeDetector.GrabPoint.y - player.transform.position.y;

                if (height <= 1f)
                    player.Anim.SetTrigger("StepUpQtr");
                else if (height <= 1.7f)
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
        if (!waitingBool && isRootMotion && animState.IsName("Locomotion"))
        {
            player.Anim.applyRootMotion = false;
            player.EnableCharControl();
            isRootMotion = false;
        }
        else if (waitingBool && (animState.IsName("StepUp_Hlf") || animState.IsName("StepUp_Qtr")
            || animState.IsName("StepUp_Full")))
        {
            player.DisableCharControl();

            float startTime, endTime;
            Quaternion rotation = Quaternion.LookRotation(ledgeDetector.Direction, Vector3.up);
            if (animState.IsName("StepUp_Qtr"))
            {
                startTime = 0.1f;
                endTime = 0.62f;
            }
            else if (animState.IsName("StepUp_Half"))
            {
                startTime = 0.23f;
                endTime = 0.9f;
            }
            else
            {
                startTime = 0.14f;
                endTime = 0.99f; 
            }

            player.Anim.MatchTarget(ledgeDetector.GrabPoint + (player.transform.forward * 0.1f),
                    player.transform.rotation, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.one, 1f), 
                    startTime, endTime);

            waitingBool = false;
        }
    }
}
