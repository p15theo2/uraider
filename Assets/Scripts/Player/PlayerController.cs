using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerSFX))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float sprintSpeed = 5f;
    public float runSpeed = 3f;
    public float swimSpeed = 2f;
    public float treadSpeed = 1.2f;
    public float walkSpeed = 1.2f;
    [Header("Physics")]
    public float gravity = 14f;
    [Header("Jump Speeds")]
    public float jumpYVel = 5.8f;
    public float jumpZVel = 4f;
    public float sJumpYVel = 3f;
    public float sJumpZVel = 2.4f;
    [Header("Smoothing")]
    public float interpolationRate = 8f;

    [Header("Object References")]
    public CameraController camController;
    public Transform rhAimPoint;
    public Transform lhAimPoint;
    public GameObject pistolLHand;
    public GameObject pistolRHand;
    public GameObject pistolLLeg;
    public GameObject pistolRLeg;

    private IPlayerState currentState;
    private CharacterController charControl;
    private Transform cam;
    private Animator anim;
    private PlayerStats playerStats;
    private PlayerSFX playerSFX;
    private Weapon[] pistols = new Weapon[2];

    private bool isGrounded = true;
    private bool rhAim = false;
    private bool lhAim = false;
    private Vector3 velocity;

    private void Start()
    {
        charControl = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        anim = GetComponent<Animator>();
        playerSFX = GetComponent<PlayerSFX>();
        pistols[0] = pistolLHand.GetComponent<Weapon>();
        pistols[1] = pistolRHand.GetComponent<Weapon>();
        playerStats = GetComponent<PlayerStats>();
        playerStats.HideCanvas();
        velocity = Vector3.zero;
        currentState = Locomotion.Instance;
        currentState.OnEnter(this);
    }

    private void Update()
    {
        isGrounded = charControl.isGrounded && velocity.y <= 0.0f;
        anim.SetBool("isGrounded", isGrounded);

        currentState.Update(this);

        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);
        float animTime = animState.normalizedTime <= 1.0f ? animState.normalizedTime
            : animState.normalizedTime % (int)animState.normalizedTime;
        anim.SetFloat("AnimTime", animTime);  // Used for determining certain transitions

        if (charControl.enabled)
            charControl.Move(velocity * Time.deltaTime);
    }

    public void MoveGrounded(float speed, bool pushDown = true)
    {
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cam.right;

        Vector3 targetVector = camForward * Input.GetAxisRaw("Vertical")
            + camRight * Input.GetAxisRaw("Horizontal");
        if (targetVector.magnitude > 1.0f)
            targetVector = targetVector.normalized;
        targetVector.y = 0f;
        targetVector *= speed;

        velocity.y = 0f; // So slerp is correct when pushDown is true

        if (velocity.magnitude < 0.1f && targetVector.magnitude > 0f)
        {
            velocity = transform.forward * 0.1f;
        }

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * interpolationRate);

        anim.SetFloat("Speed", UMath.GetHorizontalMag(velocity));
        anim.SetFloat("SignedSpeed", UMath.GetHorizontalMag(velocity)
            * Mathf.Sign(Input.GetAxisRaw("Vertical")));
        anim.SetFloat("TargetSpeed", UMath.GetHorizontalMag(targetVector));

        if (pushDown)
            velocity.y = -gravity;  // so charControl is grounded consistently
    }

    public void MoveFree(float speed)
    {
        Vector3 targetVector = cam.forward * Input.GetAxisRaw("Vertical")
            + cam.right * Input.GetAxisRaw("Horizontal");
        if (targetVector.magnitude > 1.0f)
            targetVector = targetVector.normalized;
        targetVector *= speed;

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * interpolationRate);

        anim.SetFloat("Speed", UMath.GetHorizontalMag(velocity));
        anim.SetFloat("TargetSpeed", UMath.GetHorizontalMag(targetVector));
    }

    public void RotateToVelocityGround()
    {
        if (UMath.GetHorizontalMag(velocity) > 0.1f)
        {
            Quaternion target = Quaternion.Euler(0.0f, Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg, 0.0f);
            transform.rotation = target;
        }
    }

    public void RotateToVelocity()
    {
        if (UMath.GetHorizontalMag(velocity) > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    public void RotateToTarget(Vector3 target)
    {
        Vector3 direction = Vector3.Scale((target - transform.position), new Vector3(1.0f, 0.0f, 1.0f));
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    public void ApplyGravity(float amount)
    {
        velocity.y -= amount * Time.deltaTime;
    }

    public void FireRightPistol()
    {
        pistols[1].Fire();
    }

    public void FireLeftPistol()
    {
        pistols[0].Fire();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (rhAim)
        {
            anim.SetIKPosition(AvatarIKGoal.RightHand, rhAimPoint.position);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
        }
        if (lhAim)
        {
            anim.SetIKPosition(AvatarIKGoal.LeftHand, lhAimPoint.position);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
        }
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

    public PlayerSFX SFX
    {
        get { return playerSFX; }
    }

    public PlayerStats Stats
    {
        get { return playerStats; }
    }

    public bool Grounded
    {
        get { return isGrounded; }
    }

    public bool RHAim
    {
        get { return rhAim; }
        set { rhAim = value; }
    }

    public bool LHAim
    {
        get { return lhAim; }
        set { lhAim = value; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }
}
