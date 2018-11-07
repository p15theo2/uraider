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
        player.EnableCharControl();
        player.Anim.applyRootMotion = false;
        player.ForceWaistRotation = true;
        player.Anim.SetBool("isCombat", true);
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
        target = null;
        player.Anim.applyRootMotion = false;
        player.camController.State = CameraState.Grounded;
        player.WaistTarget = null;
        player.ForceWaistRotation = false;
        player.Anim.SetBool("isCombat", false);
        player.Stats.HideCanvas();
        player.Anim.SetBool("isTargetting", false);
        player.Anim.SetBool("isFiring", false);
        player.pistolLHand.SetActive(false);
        player.pistolRHand.SetActive(false);
        player.pistolLLeg.SetActive(true);
        player.pistolRLeg.SetActive(true);
    }

    public override void Update(PlayerController player)
    {
        if (!Input.GetKey(player.playerInput.drawWeapon) && Input.GetAxisRaw("CombatTrigger") < 0.1f)
        {
            player.StateMachine.GoToState<Locomotion>();
            return;
        }

        if (player.groundDistance > 0.6f && !player.Grounded)
        {
            player.Velocity = Vector3.Scale(player.Velocity, new Vector3(1f, 0f, 1f));
            player.StateMachine.GoToState<InAir>();
            return;
        }

        player.Anim.SetFloat("Stairs", 0f, 0.1f, Time.deltaTime);

        float moveSpeed = Input.GetKey(player.playerInput.walk) ? player.walkSpeed
            : player.runSpeed;

        player.WaistRotation = player.transform.rotation;
        player.Anim.SetBool("isTargetting", true);

        player.MoveStrafeGround(moveSpeed);
        if (player.targetSpeed > 1f)
            player.RotateToVelocityGround2();

        if (target == null)
            CheckForTargets(player);

        player.camController.State = target == null ? CameraState.Grounded : CameraState.Combat;
        
        /*if (player.CombatAngle <= 90f || player.CombatAngle > 135f)
        {*/
            
       /* }
        else
        {
            Vector3 forwardMod = new Vector3(player.Cam.forward.x, 0f, player.Cam.forward.z).normalized;

            targetRot = Quaternion.LookRotation(
                Quaternion.Euler(0f, 90f * Mathf.Sign(player.CombatAngle), 0f) * forwardMod,
                Vector3.up);
        }*/

        player.Anim.SetBool("isFiring", Input.GetKey(player.playerInput.fireWeapon));
    }

    private void CheckForTargets(PlayerController player)
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, 10f);
        foreach (Collider c in hitColliders)
        {
            if (c.gameObject.CompareTag("Enemy"))
            {
                player.camController.LookAt = target = c.gameObject.transform;
                break;
            }
            else
            {
                target = null;
            }
        }
    }
}
