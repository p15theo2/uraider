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

        if (target == null)
            CheckForTargets(player);

        float moveSpeed = Input.GetKey(player.playerInput.walk) ? player.walkSpeed
            : player.runSpeed;

        float multiplier = Input.GetKey(player.playerInput.walk) ? 1.32f
            : 3.36f;

        float right = Input.GetAxis(player.playerInput.horizontalAxis) * multiplier;
        float forward = Input.GetAxis(player.playerInput.verticalAxis) * multiplier;
        Vector3 direction = player.transform.forward * forward + player.transform.right * right;

        player.Anim.SetFloat("Right", right, 0.1f, Time.deltaTime);
        player.Anim.SetFloat("Speed", forward, 0.1f, Time.deltaTime);
        player.Velocity = direction;

        //player.MoveStrafeGround(moveSpeed);

        /*if (Input.GetButtonDown("Jump"))
        {
            player.StateMachine.GoToState<Jumping>();
            return;
        }*/

        if (target == null)
        {
            player.RotateToCamera();
            //player.RotateToTarget(target.position);
            //player.camController.ForceDirection = (target.position - player.transform.position).normalized;
            //player.RotateToVelocityGround(5f);
            player.WaistRotation = /*Quaternion.LookRotation(player.transform.forward, Vector3.up)*/player.transform.rotation;
            //player.camController.State = CameraState.Combat;
            player.Anim.SetBool("isTargetting", true);
        }
        else
        {
            player.camController.ForceDirection = Vector3.zero;
            player.WaistRotation = player.transform.rotation;
            player.camController.State = CameraState.Grounded;
            player.RotateToVelocityGround();
            player.Anim.SetBool("isTargetting", false);
        }

        player.Anim.SetBool("isFiring", Input.GetKey(player.playerInput.fireWeapon));
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
