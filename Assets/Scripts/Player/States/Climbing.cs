using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : PlayerStateBase<Climbing>
{
    private bool ledgeLeft;
    private bool ledgeInnerLeft;
    private bool ledgeRight;
    private bool isOutCornering = false;
    private bool isInCornering = false;
    private bool isClimbingUp = false;
    private bool isFeetRoom = false;
    private float right = 0f;
    private float grabForwardOffset = 0.1f;
    private float grabUpOffset = 2.1f; // 1.78
    private float bracedForwardOffset = 0.32f;
    private float bracedUpOffset = 1f;

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
            if (animState.IsName("InCornerLeft") || animState.IsName("CornerLeft"))
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

            if (animState.IsName("ClimbUp"))
            {
                Vector3 matchPoint = ledgeDetector.GrabPoint + player.transform.forward * 0.18f;
                player.Anim.MatchTarget(matchPoint, player.transform.rotation, AvatarTarget.Root, 
                    new MatchTargetWeightMask(Vector3.one, 1f), 0.03f, 0.9f);
            }
            else if (animState.IsName("Locomotion"))
                player.State = Locomotion.Instance;

            return;
        }

        right = Input.GetAxisRaw("Horizontal");
        player.Anim.SetFloat("Right", right);

        CheckForFeetRoom(player);
        HandleCorners(player);
        AdjustPosition(player);

        if (Input.GetKey(KeyCode.Space) && !isOutCornering && !isClimbingUp && right == 0.0f
            && ledgeDetector.CanClimbUp(player.transform.position, player.transform.forward))
            ClimbUp(player);

        if (Input.GetKey(KeyCode.LeftShift) && !isOutCornering && !isInCornering && !isClimbingUp
            && right == 0f)
            LetGo(player);
    }

    private void HandleCorners(PlayerController player)
    {
        Vector3 start = player.transform.position + (Vector3.up * 1.75f) - (player.transform.right * 0.24f);
        ledgeLeft = ledgeDetector.FindLedgeAtPoint(start, player.transform.forward, 1.0f, 1.0f);

        start = player.transform.position + (Vector3.up * 1.75f) - (player.transform.forward * 0.15f);
        ledgeInnerLeft = ledgeDetector.FindLedgeAtPoint(start, -player.transform.right, 0.34f, 1.0f);

        if (!ledgeLeft && right < -0.1f)
        {
            player.Anim.applyRootMotion = false; // Stops player overshooting turn point
            isOutCornering = true;
        }
        else if (ledgeInnerLeft && right < -0.1f)
        {
            player.Anim.applyRootMotion = false; // Stops player overshooting turn point
            isInCornering = true;
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
        if (Input.GetKey(KeyCode.LeftControl))
            player.Anim.SetTrigger("Handstand");
        else
            player.Anim.SetTrigger("ClimbUp");
        isClimbingUp = true;
    }

    private void LetGo(PlayerController player)
    {
        player.State = InAir.Instance;
    }

    private void AdjustPosition(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        RaycastHit hit;
        Vector3 start = player.transform.position + Vector3.up * 1.7f;
        Debug.DrawRay(start, player.transform.forward * 0.5f, Color.green);
        
        if (Physics.Raycast(start, player.transform.forward, out hit, 0.5f))
        {
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation,
                Quaternion.LookRotation(-hit.normal, Vector3.up), 10f * Time.deltaTime);

            player.transform.position = new Vector3(
                hit.point.x 
                - (player.transform.forward.x * (animState.IsName("BracedHang") ? bracedForwardOffset : grabForwardOffset )),

                player.transform.position.y,

                hit.point.z 
                - (player.transform.forward.z * (animState.IsName("BracedHang") ? bracedForwardOffset : grabForwardOffset))
                );
        }
    }

    private void CheckForFeetRoom(PlayerController player)
    {
        player.Anim.SetBool("isFeetRoom", isFeetRoom = 
            Physics.Raycast(player.transform.position, player.transform.forward, 0.5f));
    }
}
