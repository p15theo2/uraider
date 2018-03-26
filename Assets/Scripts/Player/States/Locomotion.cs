using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locomotion : PlayerStateBase<Locomotion>
{
    private const float INTER_RATE = 8.0f;

    private Vector3 velocity;

    public override void OnEnter(PlayerController player)
    {
        velocity = UMath.GetHorizontalMag(velocity) > player.runSpeed ?
            player.Velocity.normalized * player.runSpeed
            : player.Velocity;

        player.Anim.applyRootMotion = false;
    }

    public override void OnExit(PlayerController player)
    {
        
    }

    public override void Update(PlayerController player)
    {
        HandleMovement(player);
        HandleRotation(player);

        if (Input.GetKeyDown(KeyCode.Space))
            player.State = Jumping.Instance;
    }

    private void HandleRotation(PlayerController player)
    {
        if (velocity.magnitude > 0.1f)
        {
            Quaternion target = Quaternion.Euler(0.0f, Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg, 0.0f);
            player.transform.rotation = target;
        }
    }

    private void HandleMovement(PlayerController player)
    {
        Vector3 camForward = Vector3.Scale(player.Cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = player.Cam.right;

        float moveSpeed = Input.GetKey(KeyCode.LeftAlt) ? player.walkSpeed
            : Input.GetKey(KeyCode.LeftControl) ? player.sprintSpeed
            : player.runSpeed;

        Vector3 targetVector = camForward * Input.GetAxisRaw("Vertical")
            + camRight * Input.GetAxisRaw("Horizontal");
        if (targetVector.magnitude > 1.0f)
            targetVector = targetVector.normalized;
        targetVector.y = -9.81f;  // Ensures charControl reports grounded correctly
        targetVector *= moveSpeed;

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * INTER_RATE);
        player.Anim.SetFloat("Speed", UMath.GetHorizontalMag(velocity));

        player.Velocity = velocity;
    }
}
