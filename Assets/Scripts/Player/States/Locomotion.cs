using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : StateBase<PlayerController>
{
    private bool isRootMotion = false;  // Used for root motion of step ups
    private bool waitingBool = false;  // avoids early reset of root mtn

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.applyRootMotion = false;
    }

    public override void OnExit(PlayerController player)
    {
        isRootMotion = false;
    }

    public override void Update(PlayerController player)
    {
        if (!player.Grounded && !isRootMotion)
        {
            player.StateMachine.GoToState<InAir>();
            return;
        }
        else if (Input.GetButtonDown("Draw Weapon"))
        {
            player.StateMachine.GoToState<Combat>();
            return;
        }

        float moveSpeed = Input.GetButton("Walk") ? player.walkSpeed
            : player.runSpeed;

        player.Anim.SetFloat("Forward", Input.GetAxisRaw("Vertical"));

        player.MoveGrounded(moveSpeed);
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        player.RotateToVelocityGround();

        HandleLedgeStepMotion(player);
        LookForStepLedges(player);

        if (Input.GetButtonDown("Crouch"))
        {
            Vector3 start = player.transform.position
                + player.transform.forward * 0.2f
                + Vector3.down * 0.2f;
            if (ledgeDetector.FindLedgeAtPoint(start, -player.transform.forward, 0.4f, 0.4f))
            {
                player.DisableCharControl();
                player.Anim.SetTrigger("ToLedgeForward");
                player.Anim.applyRootMotion = true;
                player.StateMachine.GoToState<Climbing>();
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
                    rotation, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.one, 1f), 
                    startTime, endTime);

            waitingBool = false;
        }
    }
}
