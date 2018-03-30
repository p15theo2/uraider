using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swimming : PlayerStateBase<Swimming>
{
    private bool isEntering = false;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isSwimming", true);
        player.camController.PivotOnTarget();
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isSwimming", false);
        isEntering = false;
        player.camController.PivotOnPivot();
    }

    public override void Update(PlayerController player)
    {
        if (isEntering)
        {
            if (player.Velocity.y < 0f)
                player.ApplyGravity();
            else
                isEntering = false;

            return;
        }

        if (Input.GetKey(KeyCode.Space))
            SwimUp(player);
        else if (Input.GetKey(KeyCode.LeftShift))
            SwimDown(player);
        else
            player.MoveFree();

        if (player.Velocity.magnitude > 0.1f)
            player.RotateToVelocity();
        else
            player.RotateToVelocityGround();
    }

    private void SwimUp(PlayerController player)
    {

    }

    private void SwimDown(PlayerController player)
    {

    }
}
