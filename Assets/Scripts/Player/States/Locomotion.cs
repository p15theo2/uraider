using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : PlayerStateBase<Locomotion>
{
    private const float INTER_RATE = 8.0f;

    private bool isCrouch = false;
    private bool isRootMotion = false;  // Used for root motion of step ups
    private bool waitingBool = false;  // avoids early reset of root mtn

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.applyRootMotion = false;
    }

    public override void OnExit(PlayerController player)
    {
        isCrouch = false;
        isRootMotion = false;
    }

    public override void Update(PlayerController player)
    {
        if (!player.Grounded && !isRootMotion)
        {
            player.State = InAir.Instance;
            return;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            player.State = Combat.Instance;
            return;
        }

        float moveSpeed = Input.GetKey(KeyCode.LeftControl) ? player.walkSpeed
            : player.runSpeed;

        player.MoveGrounded(moveSpeed);
        player.RotateToVelocityGround();

        HandleLedgeStepMotion(player);
        LookForStepLedges(player);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Vector3 start = player.transform.position
                + player.transform.forward * 0.2f
                + Vector3.down * 0.2f;
            if (ledgeDetector.FindLedgeAtPoint(start, -player.transform.forward, 0.4f, 0.4f))
            {
                player.DisableCharControl();
                player.Anim.SetTrigger("ToLedgeForward");
                player.Anim.applyRootMotion = true;
            }
            else
            {
                isCrouch = Input.GetKey(KeyCode.LeftShift);
                player.Anim.SetBool("isCrouch", isCrouch);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isRootMotion)
            player.State = Jumping.Instance;
    }

    private void LookForStepLedges(PlayerController player)
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRootMotion)
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
