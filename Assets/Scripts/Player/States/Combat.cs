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
    }

    public override void Update(PlayerController player)
    {
        player.Anim.SetFloat("Stairs", 0f, 0.1f, Time.deltaTime);

        if (!Input.GetKey(player.playerInput.drawWeapon) && Input.GetAxisRaw("CombatTrigger") < 0.1f)
        {
            player.Anim.SetBool("isCombat", false);
            player.Anim.SetBool("isTargetting", false);
            player.Anim.SetBool("isFiring", false);
            player.Stats.HideCanvas();
            player.pistolLHand.SetActive(false);
            player.pistolRHand.SetActive(false);
            player.pistolLLeg.SetActive(true);
            player.pistolRLeg.SetActive(true);
            player.ForceWaistRotation = false;
            player.StateMachine.GoToState<Locomotion>();
            return;
        }

        if (player.Grounded)
        {
            if (Input.GetKeyDown(player.playerInput.jump))
            {
                player.StateMachine.GoToState<CombatJumping>();
                return;
            }

            float moveSpeed = Input.GetKey(player.playerInput.walk) ? player.walkSpeed
            : player.runSpeed;

            player.MoveStrafeGround(moveSpeed);
            if (player.TargetSpeed > 1f)
                player.RotateToVelocityStrafe();
        }
        else
        {
            player.ApplyGravity(player.gravity);
        }

        if (target == null)
            CheckForTargets(player);

        if (target != null)
        {
            player.Anim.SetFloat("AimAngle", 
                Vector3.SignedAngle((target.position - player.transform.position).normalized, 
                player.transform.forward, Vector3.up));
            Debug.Log("AimAngle " + player.Anim.GetFloat("AimAngle"));
        }
        else
        {
            player.Anim.SetFloat("AimAngle", player.CombatAngle);
        }

        player.WaistRotation = player.transform.rotation;
        player.Anim.SetBool("isTargetting", true);

        player.camController.State = target == null ? CameraState.Grounded : CameraState.Combat;

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
