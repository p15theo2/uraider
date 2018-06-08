using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : StateBase<PlayerController>
{
    private Transform target;
    private Weapon leftPistol;
    private Weapon rightPistol;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isCombat", true);
        player.Anim.applyRootMotion = false;
        player.Stats.ShowCanvas();
        player.pistolLHand.SetActive(true);
        player.pistolRHand.SetActive(true);
        player.pistolLLeg.SetActive(false);
        player.pistolRLeg.SetActive(false);
        leftPistol = player.pistolLHand.GetComponent<Weapon>();
        rightPistol = player.pistolRHand.GetComponent<Weapon>();
    }

    public override void OnExit(PlayerController player)
    {
        player.camController.State = CameraState.Grounded;
        player.WaistTarget = null;
        player.Anim.SetBool("isCombat", false);
        player.Stats.HideCanvas();
        player.Anim.SetBool("isTargetting", false);
        player.pistolLHand.SetActive(false);
        player.pistolRHand.SetActive(false);
        player.pistolLLeg.SetActive(true);
        player.pistolRLeg.SetActive(true);
    }

    public override void Update(PlayerController player)
    {
        if (!Input.GetButton("Draw Weapon"))
        {
            player.StateMachine.GoToState<Locomotion>();
            return;
        }

        CheckForTargets(player);

        float moveSpeed = Input.GetButton("Walk") ? player.walkSpeed
            : player.runSpeed;

        player.Anim.SetFloat("Right", Input.GetAxis("Horizontal"));
        player.Anim.SetFloat("Forward", Input.GetAxis("Vertical"));

        player.MoveGrounded(moveSpeed);

        if (Input.GetButtonDown("Jump"))
            player.StateMachine.GoToState<Jumping>();

        if (target != null)
        {
            player.RotateToTarget(target.position);
            player.WaistTarget = target;
            player.camController.State = CameraState.Combat;
            player.Anim.SetBool("isTargetting", true);
            if (Input.GetMouseButtonDown(0))
            {
                player.Anim.SetBool("isFiring", true);
                leftPistol.Target = target.position;
                rightPistol.Target = target.position;
            }
            else
            {
                player.Anim.SetBool("isFiring", false);
            }
        }
        else
        {
            player.WaistTarget = null;
            player.camController.State = CameraState.Grounded;
            player.RotateToVelocityGround();
            player.Anim.SetBool("isTargetting", false);
        }
    }

    private void CheckForTargets(PlayerController player)
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, 10f);
        foreach (Collider c in hitColliders)
        {
            if (c.gameObject.CompareTag("Enemy"))
            {
                target = c.gameObject.transform;
                break;
            }
            else
            {
                target = null;
            }
        }
    }
}
