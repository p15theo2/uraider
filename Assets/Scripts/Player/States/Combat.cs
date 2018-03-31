using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : PlayerStateBase<Combat>
{
    private Transform target;
    private Weapon leftPistol;
    private Weapon rightPistol;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isCombat", true);
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
        player.RHAim = false;
        player.LHAim = false;
        player.Anim.SetBool("isCombat", false);
        player.Anim.SetBool("isTargetting", false);
        player.pistolLHand.SetActive(false);
        player.pistolRHand.SetActive(false);
        player.pistolLLeg.SetActive(true);
        player.pistolRLeg.SetActive(true);
    }

    public override void Update(PlayerController player)
    {
        if (!Input.GetMouseButton(1))
        {
            player.State = Locomotion.Instance;
            return;
        }

        CheckForTargets(player);

        float moveSpeed = Input.GetKey(KeyCode.LeftControl) ? player.walkSpeed
            : player.runSpeed;

        player.MoveGrounded(moveSpeed);

        if (target != null)
        {
            player.RotateToTarget(target.position);
            player.camController.State = CameraState.Combat;
            player.Anim.SetBool("isTargetting", true);
            player.RHAim = true;
            player.LHAim = true;
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
            player.camController.State = CameraState.Grounded;
            player.RotateToVelocityGround();
            player.Anim.SetBool("isTargetting", false);
            player.RHAim = false;
            player.LHAim = false;
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
