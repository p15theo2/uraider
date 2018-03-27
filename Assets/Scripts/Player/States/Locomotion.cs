using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : PlayerStateBase<Locomotion>
{
    private const float INTER_RATE = 8.0f;

    private bool isCrouch = false;
    private bool isRootMotion = false;  // Used for root motion of step ups
    private bool waitingBool = false;  // avoids early reset of root mtn

    private Vector3 velocity;
    private LedgeDetector ledgeDetector = new LedgeDetector();

    public override void OnEnter(PlayerController player)
    {
        velocity = UMath.GetHorizontalMag(player.Velocity) > player.runSpeed ?
            player.Velocity.normalized * player.runSpeed
            : player.Velocity;

        player.Anim.applyRootMotion = false;
    }

    public override void OnExit(PlayerController player)
    {
        isCrouch = false;
        isRootMotion = false;
    }

    public override void Update(PlayerController player)
    {
        HandleMovement(player);
        HandleRotation(player);
        
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        if (!waitingBool && animState.IsName("Locomotion") && isRootMotion)
        {
            player.Anim.applyRootMotion = false;
            player.EnableCharControl();
            isRootMotion = false;
        }
        else if (animState.IsName("StepUp_Hlf") || animState.IsName("StepUp_Qtr")
            || animState.IsName("StepUp_Full"))
        {
            player.DisableCharControl();
            if (animState.IsName("StepUp_Qtr"))
            {
                player.Anim.MatchTarget(ledgeDetector.GrabPoint + (player.transform.forward * 0.1f), player.transform.rotation, AvatarTarget.LeftFoot,
                    new MatchTargetWeightMask(Vector3.one, 1f), 0.1f, 0.35f);
            }
            else if (animState.IsName("StepUp_Half"))
            {
                player.Anim.MatchTarget(ledgeDetector.GrabPoint + (player.transform.forward * 0.1f), player.transform.rotation, AvatarTarget.LeftFoot,
                    new MatchTargetWeightMask(Vector3.one, 1f), 0.23f, 0.41f);
            }
            else
            {
                player.Anim.MatchTarget(ledgeDetector.GrabPoint + (player.transform.forward * 0.1f), player.transform.rotation, AvatarTarget.LeftFoot,
                    new MatchTargetWeightMask(Vector3.one, 1f), 0.14f, 0.69f);
            }
            waitingBool = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isRootMotion)
        {
            Vector3 startL = player.transform.position + (Vector3.up * 0.7f);
            Vector3 startM = player.transform.position + (Vector3.up * 1.2f);
            Vector3 startU = player.transform.position + (Vector3.up * 1.7f);
            // Checks for ledge hop ups without grabbing
            isRootMotion = ledgeDetector.FindLedgeAtPoint(startL, player.transform.forward, 0.34f, 0.2f)
                || ledgeDetector.FindLedgeAtPoint(startM, player.transform.forward, 0.34f, 0.2f)
                || ledgeDetector.FindLedgeAtPoint(startU, player.transform.forward, 0.34f, 0.2f);

            if (isRootMotion)
            {
                player.Anim.applyRootMotion = true;
                float height = ledgeDetector.GrabPoint.y - player.transform.position.y;

                if (height < 0.9f)
                    player.Anim.SetTrigger("StepUpQtr");
                else if (height < 1.3f)
                    player.Anim.SetTrigger("StepUpHlf");
                else
                    player.Anim.SetTrigger("StepUpFull");

                waitingBool = true;
            }
        }

        isCrouch = Input.GetKey(KeyCode.LeftShift);
        player.Anim.SetBool("isCrouch", isCrouch);

        if (Input.GetKeyDown(KeyCode.Space) && !isRootMotion)
            player.State = Jumping.Instance;
    }

    private void HandleRotation(PlayerController player)
    {
        if (velocity.magnitude > 0.1f)
        {
            Quaternion target = Quaternion.Euler(0.0f, Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg, 0.0f);
            player.transform.rotation = target;
        }
    }

    private void HandleMovement(PlayerController player)
    {
        Vector3 camForward = Vector3.Scale(player.Cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = player.Cam.right;

        float moveSpeed = Input.GetKey(KeyCode.LeftControl) ? player.walkSpeed
            : player.runSpeed;

        Vector3 targetVector = camForward * Input.GetAxisRaw("Vertical")
            + camRight * Input.GetAxisRaw("Horizontal");
        if (targetVector.magnitude > 1.0f)
            targetVector = targetVector.normalized;
        targetVector.y = -9.81f;  // Ensures charControl reports grounded correctly
        targetVector *= moveSpeed;

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * INTER_RATE);
        player.Anim.SetFloat("Speed", UMath.GetHorizontalMag(velocity));
        player.Anim.SetFloat("TargetSpeed", UMath.GetHorizontalMag(targetVector));

        player.Velocity = velocity;
    }
}
