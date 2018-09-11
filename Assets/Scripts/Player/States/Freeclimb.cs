using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Freeclimb : StateBase<PlayerController>
{
    private bool isClimbingUp = false;
    private bool isSlantClimb = false;
    private bool isTransition = false;
    private bool isGettingOff = false;
    private float forwardOffset = 0.5f;

    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.Velocity = Vector3.zero;
        player.MinimizeCollider();
        player.DisableCharControl();
        player.Anim.applyRootMotion = true;
        player.Anim.SetBool("isFreeclimb", true);
    }

    public override void OnExit(PlayerController player)
    {
        player.MaximizeCollider();
        player.EnableCharControl();
        isClimbingUp = false;
        isSlantClimb = false;
        isGettingOff = false;
        player.Anim.applyRootMotion = false;
        player.Anim.SetBool("isFreeclimb", false);
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Debug.Log(isClimbingUp);

        if (isTransition)
        {
            Debug.Log("trans me");
            if (animState.IsName("Freeclimb_to_Slant") || animState.IsName("Slantclimb_to_Freeclimb")
                || animState.IsName("Slantclimb_to_Freeclimb_Right"))
            {
                player.Anim.applyRootMotion = true;
                return;
            }
            else if (animState.IsName("Slantclimb_Idle") || animState.IsName("Freeclimb_Idle"))
            {
                isTransition = false;
            }
        }

        if (isClimbingUp)
        {
            Debug.Log("I climb up now");
            if (animState.IsName("Locomotion"))
            {
                player.Anim.SetBool("isClimbingUp", false);
                player.StateMachine.GoToState<Locomotion>();
            }
            return;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            player.StateMachine.GoToState<InAir>();
            return;
        }

        RaycastHit hitTop;
        Vector3 slantCheckStart = player.transform.position + 1.5f * Vector3.up;
        Vector3 flatCheckStart = player.transform.position + 2f * Vector3.up - player.transform.forward * 0.2f;
        if (vertical > 0.1f && ledgeDetector.FindLedgeAtPoint(player.transform.position + Vector3.up * 1.4f,
            player.transform.forward,
            0.6f,
            0.2f, true))
        {
            isClimbingUp = true;
            player.Anim.SetBool("isClimbingUp", true);
        }
        else if (!isSlantClimb && Physics.Raycast(slantCheckStart, Vector3.up, out hitTop, 0.42f))
        {
            isSlantClimb = true;
            isTransition = true;
            player.Anim.applyRootMotion = false;
            player.Anim.SetTrigger("SlantClimb");
            player.transform.position = new Vector3(player.transform.position.x,
                hitTop.point.y - 1.7f,
                player.transform.position.z);
            return;
        }
        else if (isSlantClimb && Physics.Raycast(flatCheckStart, player.transform.forward, out hitTop, 0.205f))
        {
            if (hitTop.normal.y == 0f)
            {
                isSlantClimb = false;
                isTransition = true;
                player.Anim.applyRootMotion = false;
                player.Anim.SetTrigger("SlantClimb");
                /*player.transform.position = new Vector3(hitTop.point.x,
                    player.transform.position.y,
                    hitTop.point.z);*/
                return;
            }
            
        }

        if (player.groundDistance <= 0.6f)
            vertical = Mathf.Clamp01(vertical);

        player.Anim.SetFloat("Forward", vertical);
        player.Anim.SetFloat("Right", horizontal);

        RaycastHit hit;
        Vector3 start = player.transform.position + Vector3.up * 1.4f;
        if (Physics.Raycast(start, player.transform.forward, out hit, 1f) && !isSlantClimb
            && !(animState.IsName("FreeclimbStart") || animState.IsName("FreeClimb_Idle") || animState.IsName("Grab") || animState.IsName("Reach")))
        {
            Vector3 newPos = new Vector3(hit.point.x - player.transform.forward.x * forwardOffset,
                player.transform.position.y,
                hit.point.z - player.transform.forward.z * forwardOffset);

            player.transform.position = newPos;
            player.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
        }

        if (player.groundDistance <= 1f)
        {
            /*if (!isGettingOff)
            {
                player.Anim.SetBool("isDismounting", true);
                //player.Anim.applyRootMotion = false;
                isGettingOff = true;
            }
            else if (animState.IsName("Locomotion"))
                player.StateMachine.GoToState<Locomotion>();
            
            return;*/

        }
    }
}

