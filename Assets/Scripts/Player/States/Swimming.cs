using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swimming : StateBase<PlayerController>
{
    private bool isEntering = false;
    private bool isTreading = false;
    private bool isClimbingUp = false;

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isSwimming", true);
        isEntering = true;
        isClimbingUp = false;
        player.camController.PivotOnTarget();
        player.Velocity.Scale(Vector3.up);
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isSwimming", false);
        isEntering = false;
        isTreading = false;
        player.camController.PivotOnPivot();
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        if (isEntering)
        {
            if (player.Velocity.y < 0f)
                player.ApplyGravity(-player.gravity);
            else
                isEntering = false;

            return;
        }
        else if (isClimbingUp)
        {
            player.Anim.SetFloat("Speed", 0f);

            if (!player.isMovingAuto)
            {
                player.Anim.applyRootMotion = true;
                player.DisableCharControl();
            }   

            if (animState.IsName("Locomotion"))
            {
                player.EnableCharControl();
                player.StateMachine.GoToState<Locomotion>();
            }

            return;
        }

        if (!isTreading)
        {
            if (Input.GetButton("Jump"))
                SwimUp(player);
            else if (Input.GetButton("Crouch"))
                SwimDown(player);
            else
                player.MoveFree(player.swimSpeed);

            player.RotateToVelocity();

            RaycastHit hit;
            if (Physics.Raycast(player.transform.position + (Vector3.up * 0.5f), Vector3.down, out hit, 0.5f))
            {
                if (hit.transform.gameObject.CompareTag("Water"))
                {
                    isTreading = true;
                    player.Anim.SetBool("isTreading", true);
                    player.camController.PivotOnHead();
                    player.transform.position = hit.point + (1.48f * Vector3.down);
                    player.transform.rotation = Quaternion.Euler(0f, player.transform.rotation.y, 0f);
                }
            }
        }
        else
        {
            player.MoveGrounded(player.treadSpeed, false, 4f);
            player.RotateToVelocityGround();

            if (Input.GetButtonDown("Action"))
            {
                if (ledgeDetector.FindLedgeAtPoint(
                    player.transform.position + Vector3.up * player.charControl.height,
                    player.transform.forward,
                    0.4f,
                    0.2f))
                {
                    player.Anim.SetTrigger("ClimbUp");
                    isClimbingUp = true;
                    player.camController.PivotOnPivot();
                    Vector3 climbPoint = ledgeDetector.GrabPoint 
                        - (ledgeDetector.Direction * 0.56f) 
                        - Vector3.up * 1.8f; 
                    Quaternion rotation = Quaternion.LookRotation(ledgeDetector.Direction, Vector3.up);
                    player.MoveWait(climbPoint, rotation);
                }
            }
        }
    }

    private void SwimUp(PlayerController player)
    {
        player.MoveInDirection(player.swimSpeed, Vector3.up);
    }

    private void SwimDown(PlayerController player)
    {
        player.MoveInDirection(player.swimSpeed, Vector3.down);
    }
}
