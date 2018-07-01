using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorPole : StateBase<PlayerController>
{
    private bool isCrouch = false;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.applyRootMotion = true;
        player.Anim.SetBool("isHorPole", true);
        player.DisableCharControl();
        player.MinimizeCollider();
        player.transform.rotation = Quaternion.LookRotation(-Vector3.right, Vector3.up);
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isHorPole", false);
        player.EnableCharControl();
        player.MaximizeCollider();
        isCrouch = false;
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        if (Input.GetButtonDown("Crouch"))
        {
            player.StateMachine.GoToState<InAir>();
            return;
        }
        else if (Input.GetButtonDown("Walk"))
        {
            isCrouch = !isCrouch;
            player.Anim.SetBool("isCrouch", isCrouch);
        }

        Vector3 direction = HorPipe.CUR_PIPE.point2 - HorPipe.CUR_PIPE.point1;
        direction = direction.normalized;

        float progress = (player.transform.position.x - HorPipe.CUR_PIPE.point2.x) /
            (HorPipe.CUR_PIPE.point1.x - HorPipe.CUR_PIPE.point2.x);
        float zChange = progress * (HorPipe.CUR_PIPE.point1.z - HorPipe.CUR_PIPE.point2.z);

        if (animState.IsName("HorForward"))
            player.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        /*player.transform.position = new Vector3(player.transform.position.x,
            player.transform.position.y,
            HorPipe.CUR_PIPE.point1.z + zChange);*/
        player.Anim.SetFloat("Forward", Input.GetAxisRaw("Vertical"));
    }
}
