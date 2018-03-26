using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float sprintSpeed = 5.0f;
    public float runSpeed = 3.0f;
    public float walkSpeed = 1.2f;
    public float gravity = 14.0f;
    public float jumpYVel = 5.715f;
    public float jumpZVel = 5.143f;
    public float sJumpYVel = 3.2f;
    public float sJumpZVel = 2.6f;

    private IPlayerState currentState;
    private CharacterController charControl;
    private Transform cam;
    private Animator anim;

    private bool isGrounded = true;
    private Vector3 velocity;

    private void Start()
    {
        charControl = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        anim = GetComponent<Animator>();
        velocity = Vector3.zero;
        currentState = Locomotion.Instance;
        currentState.OnEnter(this);
    }

    private void Update()
    {
        isGrounded = charControl.isGrounded;
        anim.SetBool("isGrounded", isGrounded);

        currentState.Update(this);

        if (charControl.enabled)
            charControl.Move(velocity * Time.deltaTime);
    }

    public void MinimizeCollider()
    {
        charControl.radius = 0f;
    }

    public void MaximizeCollider()
    {
        charControl.radius = 0.2f;
    }

    public void DisableCharControl()
    {
        charControl.enabled = false;
    }

    public void EnableCharControl()
    {
        charControl.enabled = true;
    }

    public IPlayerState State
    {
        get { return currentState; }
        set
        {
            currentState.OnExit(this);
            currentState = value;
            currentState.OnEnter(this);
        }
    }

    public CharacterController Controller
    {
        get { return charControl; }
    }

    public Transform Cam
    {
        get { return cam; }
    }

    public Animator Anim
    {
        get { return anim; }
    }

    public bool Grounded
    {
        get { return isGrounded; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }
}
