using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Grabbing : StateBase<PlayerController>
{

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;
    private Vector3 grabPoint;
    private GrabType grabType;

    public override void OnEnter(PlayerController player)
    {
        Debug.Log("whynograb");
        player.Anim.SetBool("isGrabbing", true);
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isGrabbing", false);
    }

    public override void Update(PlayerController player)
    {
        player.ApplyGravity(player.gravity);

        if (player.Velocity.y < -10f)
        {
            player.StateMachine.GoToState<InAir>();
            return;
        }

        /*if (ledgeDetector.FindLedgeAtPoint(player.transform.position + Vector3.up * 1.7f,
            player.transform.forward,
            0.21f,
            0.2f))*/
        Vector3 startPos = new Vector3(player.transform.position.x,
            player.palmLocation.position.y,
            player.transform.position.z);
        if (ledgeDetector.FindLedgeAtPoint(startPos,
        player.transform.forward,
        0.21f,
        0.1f))
        {
            grabPoint = new Vector3(ledgeDetector.GrabPoint.x - (player.transform.forward.x * player.grabForwardOffset),
                ledgeDetector.GrabPoint.y - player.grabUpOffset,
                ledgeDetector.GrabPoint.z - (player.transform.forward.z * player.grabForwardOffset));

            grabType = ledgeDetector.GetGrabType(player.transform.position, player.transform.forward,
                player.jumpZBoost, player.jumpYVel, -player.gravity);

            player.transform.position = grabPoint;

            if (ledgeDetector.WallType == LedgeType.Free)
                player.StateMachine.GoToState<Freeclimb>();
            else if (grabType == GrabType.Hand)
                player.StateMachine.GoToState<Climbing>();
            else
                player.StateMachine.GoToState<Locomotion>();
        }
        else if (player.Grounded)
            player.StateMachine.GoToState<Locomotion>();
    }
}

