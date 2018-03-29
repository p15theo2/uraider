using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : PlayerStateBase<Combat>
{
    private Transform target;

    public override void OnEnter(PlayerController player)
    {
        
    }

    public override void OnExit(PlayerController player)
    {
        player.camController.State = CameraState.Grounded;
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
        }
        else
        {
            Debug.Log("No Target");
            player.camController.State = CameraState.Grounded;
            player.RotateToVelocityGround();
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
