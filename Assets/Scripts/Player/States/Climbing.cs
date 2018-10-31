using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : StateBase<PlayerController>
{
    private bool ledgeLeft;
    private bool ledgeInnerLeft;
    private bool ledgeRight;
    private bool ledgeInnerRight;
    private bool isOutCornering = false;
    private bool isInCornering = false;
    private bool isClimbingUp = false;
    private bool isFeetRoom = false;
    private float right = 0f;

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.Velocity = Vector3.zero;
        player.MinimizeCollider();
        player.DisableCharControl();
        player.Anim.SetBool("isClimbing", true);
        player.Anim.applyRootMotion = true;
    }

    public override void OnExit(PlayerController player)
    {
        player.MaximizeCollider();
        player.EnableCharControl();
        player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isClimbing", false);
        isOutCornering = false;
        isInCornering = false;
        isClimbingUp = false;
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        right = Input.GetAxisRaw(player.playerInput.horizontalAxis);

        if (isInCornering || isOutCornering)
        {
            if (animState.IsName("InCornerLeft") || animState.IsName("CornerLeft")
                || animState.IsName("CornerRight") || animState.IsName("InCornerRight"))
            {
                player.Anim.applyRootMotion = true;
            }
            else if (animState.IsName("HangLoop"))
            {
                isOutCornering = isInCornering = false;
            }
            player.Anim.SetFloat("Right", right);
            return;
        }
        else if (isClimbingUp)
        {
            player.Anim.SetFloat("Speed", 0f);

            if (animState.IsName("Idle"))
            {
                player.StateMachine.GoToState<Locomotion>();
            }
            
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, player.transform.forward, out hit, 0.5f))
        {
            player.Anim.SetBool("isFeetRoom", true);

            if (hit.collider.CompareTag("Freeclimb"))
            {
                player.StateMachine.GoToState<Freeclimb>();
                return;
            }
        }

        HandleCorners(player);
        AdjustPosition(player);

        player.Anim.SetFloat("Right", right);

        if (Input.GetKey(player.playerInput.jump) && !isOutCornering && !isClimbingUp && right == 0.0f
            && ledgeDetector.CanClimbUp(player.transform.position, player.transform.forward))
            ClimbUp(player);

        if (Input.GetKey(player.playerInput.crouch) && !isOutCornering && !isInCornering && !isClimbingUp
            && right == 0f)
            LetGo(player);
    }

    private void HandleCorners(PlayerController player)
    {
        Vector3 start = player.transform.position + (Vector3.up * 2f) - (player.transform.right * 0.24f);
        ledgeLeft = ledgeDetector.FindLedgeAtPoint(start, player.transform.forward, 0.2f, 0.2f);

        start = player.transform.position + (Vector3.up * 2f) - (player.transform.forward * 0.15f);
        ledgeInnerLeft = ledgeDetector.FindLedgeAtPoint(start, -player.transform.right, 0.34f, 0.2f);

        start = player.transform.position + (Vector3.up * 2f) + (player.transform.right * 0.24f);
        ledgeRight = ledgeDetector.FindLedgeAtPoint(start, player.transform.forward, 0.2f, 0.2f);

        start = player.transform.position + (Vector3.up * 2f) - (player.transform.forward * 0.15f);
        ledgeInnerRight = ledgeDetector.FindLedgeAtPoint(start, player.transform.right, 0.34f, 0.2f);

        if (right < -0.1f)
        {
            if (ledgeInnerLeft)
            {
                player.Anim.applyRootMotion = false; // Stops player overshooting turn point
                isInCornering = true;
            }
            else if (!ledgeLeft)
            {
                start = player.transform.position + (Vector3.up * 2f) - player.transform.right * 0.24f
                    + player.transform.forward * 0.4f;

                player.Anim.applyRootMotion = false; 
                isOutCornering = ledgeDetector.FindLedgeAtPoint(start, player.transform.right, 0.34f, 0.2f);

                if (!isOutCornering)
                    right = Mathf.Clamp01(right);
            }
            else
            {
                start = player.transform.position + (Vector3.up * 2f) - (player.transform.forward * 0.15f);

                if (Physics.Raycast(start, player.transform.right, 0.4f))
                    right = Mathf.Clamp(right, -1f, 0);
            }
        }
        else if (right > 0.1f)
        {
            if (ledgeInnerRight)
            {
                player.Anim.applyRootMotion = false; 
                isInCornering = true;
            }
            else if (!ledgeRight)
            {
                start = player.transform.position + (Vector3.up * 2f) + player.transform.right * 0.24f
                    + player.transform.forward * 0.4f;

                player.Anim.applyRootMotion = false; 
                isOutCornering = ledgeDetector.FindLedgeAtPoint(start, -player.transform.right, 0.34f, 0.2f);

                if (!isOutCornering)
                    right = Mathf.Clamp(right, -1f, 0f);
            }
            else
            {
                start = player.transform.position + (Vector3.up * 2f) - (player.transform.forward * 0.15f);

                if (Physics.Raycast(start, -player.transform.right, 0.4f))
                    right = Mathf.Clamp01(right);
            }
        }
        else
        {
            isOutCornering = isInCornering = false;
            player.Anim.applyRootMotion = true;
        }

        player.Anim.SetBool("isOutCorner", isOutCornering);
        player.Anim.SetBool("isInCorner", isInCornering);
    }

    private void ClimbUp(PlayerController player)
    {
        if (Input.GetButton("Sprint"))
            player.Anim.SetTrigger("Handstand");
        else
            player.Anim.SetTrigger("ClimbUp");

        isClimbingUp = true;
    }

    private void LetGo(PlayerController player)
    {
        player.StateMachine.GoToState<InAir>();
    }

    private void AdjustPosition(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        Debug.Log(ledgeDetector.GrabPoint.y - player.transform.position.y);

        RaycastHit hit;
        Vector3 start = player.transform.position + Vector3.up * (player.hangUpOffset - 0.1f);
        Debug.DrawRay(start, player.transform.forward * 0.4f, Color.green);
        
        if (ledgeDetector.FindLedgeAtPoint(start, player.transform.forward, 0.2f, 0.2f)
            /*Physics.Raycast(start, player.transform.forward, out hit, 0.4f)*/)
        {
            Quaternion targetRot = Quaternion.Euler(0f, Quaternion.LookRotation(ledgeDetector.Direction, Vector3.up).eulerAngles.y, 0f);

            player.transform.rotation = Quaternion.Slerp(player.transform.rotation,
                targetRot, 10f * Time.deltaTime);

            player.transform.position = new Vector3(
                ledgeDetector.GrabPoint.x
                - (player.transform.forward.x * player.hangForwardOffset),

                animState.IsName("HangLoop") ? ledgeDetector.GrabPoint.y - player.hangUpOffset : player.transform.position.y,

                ledgeDetector.GrabPoint.z
                - (player.transform.forward.z * player.hangForwardOffset)
                );
        }
    }

    private void CheckForFeetRoom(PlayerController player)
    {
        
    }
}
