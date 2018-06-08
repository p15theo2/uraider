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
    public float runSpeed = 3f;
    public float walkSpeed = 1.2f;
    public float swimSpeed = 2f;
    public float treadSpeed = 1.2f;
    [Header("Physics")]
    public float gravity = 14f;
    [Header("Jump Speeds")]
    public float jumpYVel = 5.8f;
    public float jumpZVel = 4f;
    public float sJumpYVel = 3f;
    public float sJumpZVel = 2.4f;
    [Header("Smoothing")]
    public float interpolationRate = 8f;
    [Header("IK Settings")]
    public float footYOffset = 0.1f;

    [Header("References")]
    public CameraController camController;
    public Transform waistBone;
    public Transform rightFootIK;
    public Transform leftFootIK;
    public GameObject pistolLHand;
    public GameObject pistolRHand;
    public GameObject pistolLLeg;
    public GameObject pistolRLeg;

    private bool isGrounded = true;
    private bool isFootIK = false;
    private bool holdRotation = false;
    private float targetAngle = 0f;
    private Vector3 posLastFrame = Vector3.zero;
    private float calculatedSpeed = 0f;

    private StateMachine<PlayerController> stateMachine;
    private CharacterController charControl;
    private Transform cam;
    private Animator anim;
    private PlayerStats playerStats;
    private PlayerSFX playerSFX;
    private Weapon[] pistols = new Weapon[2];
    private Transform waistTarget;
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
        stateMachine = new StateMachine<PlayerController>(this);
        SetUpStateMachine();
    }

    private void SetUpStateMachine()
    {
        stateMachine.AddState(new Locomotion());
        stateMachine.AddState(new Combat());
        stateMachine.AddState(new Climbing());
        stateMachine.AddState(new Freeclimb());
        stateMachine.AddState(new Crouch());
        stateMachine.AddState(new Dead());
        stateMachine.AddState(new InAir());
        stateMachine.AddState(new Jumping());
        stateMachine.AddState(new Swimming());
        stateMachine.GoToState<Locomotion>();
    }

    private void Update()
    {
        isGrounded = charControl.isGrounded && velocity.y <= 0.0f;
        anim.SetBool("isGrounded", isGrounded);

        stateMachine.Update();
        UpdateAnimator();

        calculatedSpeed = Mathf.Abs(Vector3.Distance(transform.position, posLastFrame)) / Time.deltaTime;

        if (charControl.enabled && anim.applyRootMotion == false)
            charControl.Move(velocity * Time.deltaTime);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (isFootIK && UMath.GetHorizontalMag(velocity) < 0.1f)
        {
            RaycastHit hit;
            if (Physics.Raycast(leftFootIK.position, Vector3.down, out hit, 0.5f))
            {
                anim.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + Vector3.up * footYOffset);
                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
                anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            }
            if (Physics.Raycast(rightFootIK.position, Vector3.down, out hit, 0.5f))
            {
                anim.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + Vector3.up * footYOffset);
                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
                anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
            }
        }
    }

    private void LateUpdate()
    {
        if (waistTarget != null)
        {
            waistBone.rotation = Quaternion.LookRotation(
                (waistTarget.position - transform.position).normalized, Vector3.up);

            // Correction for faulty bone
            waistBone.localRotation = Quaternion.Euler(
                new Vector3(waistBone.localEulerAngles.x, waistBone.localEulerAngles.y, 
                waistBone.localEulerAngles.z - 90f));
        }
    }

    private void UpdateAnimator()
    {
        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);
        float animTime = animState.normalizedTime <= 1.0f ? animState.normalizedTime
            : animState.normalizedTime % (int)animState.normalizedTime;

        anim.SetFloat("AnimTime", animTime);  // Used for determining certain transitions
    }

    public Vector3 TargetMovementVector()
    {
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cam.right;

        Vector3 targetVector = camForward * Input.GetAxisRaw("Vertical")
            + camRight * Input.GetAxisRaw("Horizontal");
        if (targetVector.magnitude > 1.0f)
            targetVector = targetVector.normalized;
        targetVector.y = 0f;

        return targetVector;
    }

    public void MoveGrounded(float speed, bool pushDown = true, float smoothing = 8f)
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
            velocity = transform.forward * 0.1f;  // Player will rotate smoothly from idle

        targetAngle = Vector3.Angle(velocity, targetVector);

        anim.SetFloat("TargetAngle", targetAngle);

        if (targetAngle > 160f && (anim.GetCurrentAnimatorStateInfo(0).IsName("Run")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("Walk")))
            holdRotation = true;
        else if (holdRotation && (anim.GetAnimatorTransitionInfo(0).IsName("Run_180 -> Run")
            || anim.GetAnimatorTransitionInfo(0).IsName("Walk_180 -> Walk")))
            holdRotation = false;

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * smoothing);
        //velocity = targetVector;

        anim.SetFloat("Speed", UMath.GetHorizontalMag(velocity));
        anim.SetFloat("TargetSpeed", UMath.GetHorizontalMag(targetVector));

        if (pushDown)
            velocity.y = -gravity;  // so charControl is grounded consistently
    }

    public void MoveFree(float speed, float smoothing = 8f)
    {
        Vector3 targetVector = cam.forward * Input.GetAxisRaw("Vertical")
            + cam.right * Input.GetAxisRaw("Horizontal");
        if (targetVector.magnitude > 1.0f)
            targetVector = targetVector.normalized;
        targetVector *= speed;

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * smoothing);

        anim.SetFloat("Speed", UMath.GetHorizontalMag(velocity));
        anim.SetFloat("TargetSpeed", UMath.GetHorizontalMag(targetVector));
    }

    public void RotateToVelocityGround(float smoothing = 0f)
    {
        if (UMath.GetHorizontalMag(velocity) > 0.1f && !holdRotation)
        {
            Quaternion target = Quaternion.Euler(0.0f, Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg, 0.0f);
            if (smoothing == 0f)
                transform.rotation = target;
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, target, smoothing * Time.deltaTime);
        }
    }

    public void RotateToVelocity(float smoothing = 0f)
    {
        if (UMath.GetHorizontalMag(velocity) > 0.1f)
        {
            if (smoothing == 0f)
                transform.rotation = Quaternion.LookRotation(velocity);
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity), 
                    smoothing * Time.deltaTime);
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

    public StateMachine<PlayerController> StateMachine
    {
        get { return stateMachine; }
    }

    public CharacterController Controller
    {
        get { return charControl; }
    }

    public Transform Cam
    {
        get { return cam; }
    }

    public Transform WaistTarget
    {
        get { return waistTarget; }
        set { waistTarget = value; }
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

    public bool IsFootIK
    {
        get { return isFootIK; }
        set { isFootIK = value; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }

    public float CalculatedSpeed
    {
        get { return calculatedSpeed; }
        set { calculatedSpeed = value; }
    }
}
