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
    private float speed;
    private float grabForwardOffset = 0.1f;
    private float grabUpOffset = 1.78f;

    private LedgeDetector ledgeDetector = new LedgeDetector();

    public override void OnEnter(PlayerController player)
    {
        player.MinimizeCollider();
        player.DisableCharControl();
        player.Anim.SetBool("isClimbing", true);
        player.Velocity = Vector3.zero;
        player.Anim.applyRootMotion = true;
    }

    public override void OnExit(PlayerController player)
    {
        player.MaximizeCollider();
        player.EnableCharControl();
        player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isClimbing", false);
        isOutCornering = false;
        isClimbingUp = false;
    }

    public override void Update(PlayerController player)
    {
        speed = Input.GetAxisRaw("Horizontal");
        player.Anim.SetFloat("Speed", speed);

        Vector3 start = player.transform.position + (Vector3.up * 1.75f) - (player.transform.right * 0.2f);
        ledgeLeft = ledgeDetector.FindLedgeAtPoint(start, player.transform.forward, 1.0f, 1.0f);
        start = player.transform.position + (Vector3.up * 1.75f);
        ledgeInnerLeft = ledgeDetector.FindLedgeAtPoint(start, -player.transform.right, 0.34f, 1.0f);

        if (!ledgeLeft && speed < -0.1f)
        {
            player.Anim.applyRootMotion = false; // Stops player overshooting turn point
            isOutCornering = true;
        }
        else if (ledgeInnerLeft && speed < -0.1f)
        {
            player.Anim.applyRootMotion = false; // Stops player overshooting turn point
            isInCornering = true;
        }
        else
        {
            isOutCornering = false;
            isInCornering = false;
        }

        player.Anim.SetBool("isOutCorner", isOutCornering);
        player.Anim.SetBool("isInCorner", isInCornering);

        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        if (animState.IsName("CornerLeft"))
        {
            player.Anim.applyRootMotion = true;
            isOutCornering = true;
        }
        else if (animState.IsName("InCornerLeft"))
        {
            player.Anim.applyRootMotion = true;
            isInCornering = true;
        }
        else if (animState.IsName("Locomotion") && isClimbingUp)
        {
            player.State = Locomotion.Instance;
        }
        else
        {
            AdjustPosition(player);
        }

        if (Input.GetKey(KeyCode.Space) && !isOutCornering && !isClimbingUp && speed == 0.0f
            && ledgeDetector.CanClimbUp(player.transform.position, player.transform.forward))
            ClimbUp(player);
    }

    private bool CheckForCorneringAt(PlayerController player, Vector3 dir, Vector3 perpDir)
    {
        Vector3 start = player.transform.position + (Vector3.up * 1.75f) - (perpDir * 0.2f);
        return ledgeDetector.FindLedgeAtPoint(start, dir, 1.0f, 1.0f);
    }

    private void ClimbUp(PlayerController player)
    {
        player.Anim.SetTrigger("ClimbUp");
        isClimbingUp = true;
    }

    private void AdjustPosition(PlayerController player)
    {
        RaycastHit hit;
        Vector3 start = player.transform.position + Vector3.up * 1.7f;
        Debug.DrawRay(start, player.transform.forward * 0.5f, Color.green);
        if (Physics.Raycast(start, player.transform.forward, out hit, 0.5f))
        {
            player.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            player.transform.position = new Vector3(hit.point.x - (player.transform.forward.x * grabForwardOffset),
                player.transform.position.y,
                hit.point.z - (player.transform.forward.z * grabForwardOffset));
        }
    }
}
