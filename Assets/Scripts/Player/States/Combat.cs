using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : PlayerStateBase<Combat>
{
    private Transform target;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isCombat", true);
        player.pistolLHand.SetActive(true);
        player.pistolRHand.SetActive(true);
        player.pistolLLeg.SetActive(false);
        player.pistolRLeg.SetActive(false);
    }

    public override void OnExit(PlayerController player)
    {
        player.camController.State = CameraState.Grounded;
        player.RHAim = false;
        player.LHAim = false;
        player.Anim.SetBool("isCombat", false);
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

        player.MoveFree();

        if (target != null)
        {
            Debug.Log("Found Target");
            player.RotateToTarget(target.position);
            player.camController.State = CameraState.Combat;
            player.Anim.SetBool("isTargetting", true);
            player.RHAim = true;
            player.LHAim = true;
        }
        else
        {
            Debug.Log("No Target");
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
