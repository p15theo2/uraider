using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Grabbing : StateBase<PlayerController>
{

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;
    private Vector3 grabPoint;
    private Vector3 lastPos;
    private GrabType grabType;

    public override void OnEnter(PlayerController player)
    {
        player.MinimizeCollider(0.01f);

        lastPos = player.transform.position;

        player.Anim.SetBool("isGrabbing", true);
    }

    public override void OnExit(PlayerController player)
    {
        player.MaximizeCollider();

        player.Anim.SetBool("isGrabbing", false);
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        player.Anim.SetFloat("YSpeed", player.Velocity.y);

        player.ApplyGravity(player.gravity);

        if (player.Velocity.y < -10f)
        {
            player.StateMachine.GoToState<InAir>();
            return;
        }

        RaycastHit hit;
        Vector3 startPos = new Vector3(player.transform.position.x,
            player.transform.position.y + (animState.IsName("Reach") ? player.grabUpOffset : 1.975f),
            player.transform.position.z);

        // If Lara's position changes too fast, can miss ledges
        float deltaH = Mathf.Max(Mathf.Abs(player.transform.position.y - lastPos.y), 0.12f);

        // Checks if there is a ledge to grab
        if (ledgeDetector.FindLedgeAtPoint(startPos,
        player.transform.forward,
        0.25f,
        deltaH))
        {
            grabPoint = new Vector3(ledgeDetector.GrabPoint.x - (player.transform.forward.x * player.grabForwardOffset),
                ledgeDetector.GrabPoint.y - (!animState.IsName("JumpUp") && !animState.IsName("Fall2") ? player.grabUpOffset : player.hangUpOffset),
                ledgeDetector.GrabPoint.z - (player.transform.forward.z * player.grabForwardOffset));

            grabType = ledgeDetector.GetGrabType(player.transform.position, player.transform.forward,
                player.jumpZBoost, player.jumpYVel, -player.gravity);

            player.transform.position = grabPoint;
            Quaternion ledgeRot = Quaternion.LookRotation(ledgeDetector.Direction, Vector3.up);
            player.transform.rotation = Quaternion.Euler(0f, ledgeRot.eulerAngles.y, 0f);

            if (ledgeDetector.WallType == LedgeType.Free)
                player.StateMachine.GoToState<Freeclimb>();
            else
                player.StateMachine.GoToState<Climbing>();
        }
        else if (Physics.Raycast(startPos, Vector3.up, out hit, 0.5f))
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

        lastPos = player.transform.position;
    }
}

