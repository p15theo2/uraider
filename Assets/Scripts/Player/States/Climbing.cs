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
    private float grabForwardOffset = 0.11f;
    private float grabUpOffset = 2.1f; // 1.78

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
            return;
        }
        else if (isClimbingUp)
        {
            player.Anim.SetFloat("Speed", 0f);

            if (animState.IsName("Locomotion"))
            {
                player.StateMachine.GoToState<Locomotion>();
            }
            
            return;
        }

        right = Input.GetAxisRaw("Horizontal");
        player.Anim.SetFloat("Right", right);

        CheckForFeetRoom(player);
        HandleCorners(player);
        AdjustPosition(player);

        if (Input.GetButton("Jump") && !isOutCornering && !isClimbingUp && right == 0.0f
            && ledgeDetector.CanClimbUp(player.transform.position, player.transform.forward))
            ClimbUp(player);

        if (Input.GetButton("Crouch") && !isOutCornering && !isInCornering && !isClimbingUp
            && right == 0f)
            LetGo(player);
    }

    private void HandleCorners(PlayerController player)
    {
        Vector3 start = player.transform.position + (Vector3.up * 1.86f) - (player.transform.right * 0.24f);
        ledgeLeft = ledgeDetector.FindLedgeAtPoint(start, player.transform.forward, 0.2f, 0.2f);

        start = player.transform.position + (Vector3.up * 1.86f) - (player.transform.forward * 0.15f);
        ledgeInnerLeft = ledgeDetector.FindLedgeAtPoint(start, -player.transform.right, 0.34f, 0.2f);

        start = player.transform.position + (Vector3.up * 1.86f) + (player.transform.right * 0.24f);
        ledgeRight = ledgeDetector.FindLedgeAtPoint(start, player.transform.forward, 0.2f, 0.2f);

        start = player.transform.position + (Vector3.up * 1.86f) - (player.transform.forward * 0.15f);
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
                player.Anim.applyRootMotion = false; 
                isOutCornering = true;
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
                player.Anim.applyRootMotion = false; 
                isOutCornering = true;
            }
        }
        else
        {
            isOutCornering = isInCornering = false;
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

        RaycastHit hit;
        Vector3 start = player.transform.position + Vector3.up * 1.86f;
        Debug.DrawRay(start, player.transform.forward * 0.4f, Color.green);
        
        if (Physics.Raycast(start, player.transform.forward, out hit, 0.4f))
        {
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation,
                Quaternion.LookRotation(-hit.normal, Vector3.up), 10f * Time.deltaTime);

            player.transform.position = new Vector3(
                hit.point.x
                - (player.transform.forward.x * grabForwardOffset),

                player.transform.position.y,

                hit.point.z
                - (player.transform.forward.z * grabForwardOffset)
                );
        }
    }

    private void CheckForFeetRoom(PlayerController player)
    {
        player.Anim.SetBool("isFeetRoom", isFeetRoom = 
            Physics.Raycast(player.transform.position, player.transform.forward, 0.5f));
    }
}
