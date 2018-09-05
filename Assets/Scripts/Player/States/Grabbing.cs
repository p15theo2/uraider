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
        RaycastHit hit;
        /*Vector3 startPos = new Vector3(player.transform.position.x,
            player.palmLocation.position.y,
            player.transform.position.z);*/
        Vector3 startPos = new Vector3(player.transform.position.x,
            player.transform.position.y + 1.72f,
            player.transform.position.z);
        if (ledgeDetector.FindLedgeAtPoint(startPos,
        player.transform.forward,
        0.24f,
        0.06f))
        {
            grabPoint = new Vector3(ledgeDetector.GrabPoint.x - (player.transform.forward.x * /*player.grabForwardOffset*/ 0.11f),
                ledgeDetector.GrabPoint.y - /*player.grabUpOffset*/ /*1.7f*/ 1.56f,
                ledgeDetector.GrabPoint.z - (player.transform.forward.z * /*player.grabForwardOffset*/ 0.11f));

            grabType = ledgeDetector.GetGrabType(player.transform.position, player.transform.forward,
                player.jumpZBoost, player.jumpYVel, -player.gravity);

            player.transform.position = grabPoint;
            player.transform.rotation = Quaternion.LookRotation(ledgeDetector.Direction, Vector3.up);

            if (ledgeDetector.WallType == LedgeType.Free)
                player.StateMachine.GoToState<Freeclimb>();
            else if (grabType == GrabType.Hand)
                player.StateMachine.GoToState<Climbing>();
            else
                player.StateMachine.GoToState<Locomotion>();
        }
        else if (Physics.Raycast(startPos, Vector3.up, out hit, 0.02f))
        {
            if (hit.collider.CompareTag("MonkeySwing"))
            {
                player.StateMachine.GoToState<MonkeySwing>();
                return;
            }
            else if (hit.collider.CompareTag("HorPole"))
            {
                player.transform.position = hit.point - Vector3.up * 1.9f;
                HorPipe.CUR_PIPE = hit.collider.gameObject.GetComponent<HorPipe>();
                player.StateMachine.GoToState<HorPole>();
                return;
            }
        }
        else if (player.Grounded)
            player.StateMachine.GoToState<Locomotion>();
    }
}

