using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerSFX))]
public class PlayerController : MonoBehaviour
{
    public bool autoLedgeTarget = true;
    public float grabTime = 0.7f;
    [Header("Movement Speeds")]
    public float sprintSpeed = 4f;
    public float runSpeed = 3.36f;
    public float walkSpeed = 1.44f;
    public float stairSpeed = 2f;
    public float swimSpeed = 2f;
    public float treadSpeed = 1.2f;
    public float slideSpeed = 2f;
    [Header("Physics")]
    public float gravity = 9.81f;
    public float deathVelocity = 12f;
    [Header("Jump Speeds")]
    public float jumpYVel = 5f;
    public float jumpZBoost = 0.8f;
    [Header("IK Settings")]
    public float footYOffset = 0.1f;
    [Header("Offsets")]
    public float grabForwardOffset = 0.11f;
    public float grabUpOffset = 1.56f;
    public float hangForwardOffset = 0.11f;
    public float hangUpOffset = 1.975f;
    [Header("Axis Names")]
    public string right = "Horizontal";
    public string forward = "Vertical";

    [Header("References")]
    public CameraController camController;
    public Transform waistBone;
    public Transform rightFootIK;
    public Transform leftFootIK;
    public Transform palmLocation;
    public GameObject pistolLHand;
    public GameObject pistolRHand;
    public GameObject pistolLLeg;
    public GameObject pistolRLeg;
    [Header("Ragdoll")]
    public Rigidbody[] ragRigidBodies;

    private bool isGrounded = true;
    private bool isSliding = false;
    private bool isFootIK = false;
    private bool holdRotation = false;
    private bool forceWaistRotation = false;
    [HideInInspector]
    public float groundDistance = 0f;
    [HideInInspector]
    public float groundAngle = 0f;
    [HideInInspector]
    public bool isMovingAuto = false;
    public float targetAngle = 0f;
    public float targetSpeed = 0f;

    private StateMachine<PlayerController> stateMachine;
    [HideInInspector]
    public CharacterController charControl;
    [HideInInspector]
    public PlayerInput playerInput;
    private Transform cam;
    private Animator anim;
    private PlayerStats playerStats;
    private PlayerSFX playerSFX;
    private Weapon[] pistols = new Weapon[2];
    private Transform waistTarget;
    private Quaternion waistRotation;
    private Vector3 velocity;
    [HideInInspector]
    public Vector3 slopeDirection;
    [HideInInspector]
    public bool useRootMotion = true;
    private RaycastHit groundHit;

    private void Awake()
    {
        DisableRagdoll();
    }

    private void Start()
    {
        charControl = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cam = camController.GetComponentInChildren<Camera>().transform;
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
        stateMachine.AddState(new Empty());
        stateMachine.AddState(new Locomotion());
        stateMachine.AddState(new Combat());
        stateMachine.AddState(new Climbing());
        stateMachine.AddState(new Freeclimb());
        stateMachine.AddState(new Drainpipe());
        stateMachine.AddState(new Ladder());
        stateMachine.AddState(new Crouch());
        stateMachine.AddState(new Dead());
        stateMachine.AddState(new InAir());
        stateMachine.AddState(new Jumping());
        stateMachine.AddState(new Swimming());
        stateMachine.AddState(new Grabbing());
        stateMachine.AddState(new AutoGrabbing());
        stateMachine.AddState(new MonkeySwing());
        stateMachine.AddState(new HorPole());
        stateMachine.AddState(new Sliding());
        stateMachine.GoToState<Locomotion>();
    }

    private void Update()
    {
        if (RingMenu.isPaused)
            return;

        CheckForGround();

        stateMachine.Update();

        UpdateAnimator();

        if (charControl.enabled)
            charControl.Move((anim.applyRootMotion ? Vector3.Scale(velocity, Vector3.up) : velocity) * Time.deltaTime);
    }

    private void CheckForGround()
    {
        // velY condition helps stop accidental grounding (like when jumping)
        isGrounded = charControl.isGrounded && velocity.y <= 0.0f;
        anim.SetBool("isGrounded", isGrounded);

        groundDistance = 2f;
        groundAngle = 0f;

        Vector3 centerStart = transform.position + Vector3.up * 0.2f;

        if ((Physics.Raycast(centerStart, Vector3.down, out groundHit, groundDistance)
            && !groundHit.collider.CompareTag("Water")))
        {
            groundDistance = transform.position.y - groundHit.point.y;
            groundAngle = UMath.GroundAngle(groundHit.normal);
        }

        anim.SetFloat("groundDistance", groundDistance);
        anim.SetFloat("groundAngle", groundAngle);
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        stateMachine.SendMessage(hit);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (isFootIK /*&& UMath.GetHorizontalMag(velocity) < 0.1f*/)
        {
            float curWeight = 1f;
            RaycastHit hit;
            if (Physics.Raycast(leftFootIK.position, Vector3.down, out hit, 0.5f))
            {
                anim.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + Vector3.up * footYOffset);
                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, curWeight);
                anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, curWeight);
            }
            if (Physics.Raycast(rightFootIK.position, Vector3.down, out hit, 0.5f))
            {
                anim.SetIKPosition(AvatarIKGoal.RightFoot, hit.point + Vector3.up * footYOffset);
                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, curWeight);
                anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, curWeight);
            }
        }
    }

    public void DisableRagdoll()
    {
        foreach (Rigidbody rb in ragRigidBodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.gameObject.GetComponent<Collider>().enabled = false;
        }
    }

    public void EnableRagdoll()
    {
        foreach (Rigidbody rb in ragRigidBodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.gameObject.GetComponent<Collider>().enabled = true;
        }
    }

    private void LateUpdate()
    {
        if (forceWaistRotation)
        {
            waistBone.rotation = waistRotation;
            
            // Correction for faulty bone
            // IF NEW MODEL CAUSES ISSUES MESS WITH THIS
            waistBone.rotation = Quaternion.Euler(
                new Vector3(waistBone.eulerAngles.x - 90f, waistBone.eulerAngles.y, 
                waistBone.eulerAngles.z/* - 90f*/));
        }
    }

    public void AnimWait(float seconds)
    {
        StartCoroutine(StopDrop(seconds));
    }

    public void MoveWait(Vector3 point, Quaternion rotation, float tRate = 1f, float rRate = 1f)
    {
        StartCoroutine(MoveTo(point, rotation, tRate, rRate));
    }

    private IEnumerator StopDrop(float secs)
    {
        float startTime = Time.time;
        anim.SetBool("isWaiting", true);
        while (Time.time - startTime < secs)
        {
            yield return null;
        }
        anim.SetBool("isWaiting", false);
    }

    private IEnumerator MoveTo(Vector3 point, Quaternion rotation, float tRate = 1f, float rRate = 1f)
    {
        anim.applyRootMotion = false;

        velocity = Vector3.zero;

        float distance = Vector3.Distance(transform.position, point);
        float difference = Quaternion.Angle(transform.rotation, rotation);
        Vector3 direction = (point - transform.position).normalized;
        bool isNotOk = true;

        isMovingAuto = true;
        anim.SetBool("isWaiting", true);

        while (isNotOk)
        {
            isNotOk = false;

            if (Mathf.Abs(distance) > 0.05f)
            {
                isNotOk = true;
                /*direction = (point - transform.position).normalized;
                velocity.y = 0f;
                velocity = Vector3.Lerp(velocity, direction * walkSpeed * tRate, 10f * Time.deltaTime);
                distance = Vector3.Distance(transform.position, point);
                anim.SetFloat("Speed", velocity.magnitude);*/
                transform.position = Vector3.Lerp(transform.position, point, tRate * Time.deltaTime);
                //anim.SetFloat("Speed", 0f, 0.4f, Time.deltaTime);
                distance = Vector3.Distance(transform.position, point);
            }
            else
            {
                velocity = Vector3.zero;
            }

            if (Mathf.Abs(difference) > 5f)
            {
                isNotOk = true;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rRate * Time.deltaTime);
                difference = Quaternion.Angle(transform.rotation, rotation);
            }

            yield return null;
        }

        transform.position = point;
        transform.rotation = rotation;
        velocity = Vector3.zero;

        isMovingAuto = false;
        anim.SetBool("isWaiting", false);
        //anim.SetFloat("Speed", 0f);
    }

    private void UpdateAnimator()
    {
        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);
        float animTime = animState.normalizedTime <= 1.0f ? animState.normalizedTime
            : animState.normalizedTime % (int)animState.normalizedTime;

        anim.SetFloat("AnimTime", animTime);  // Used for determining certain transitions
    }

    float turnValue = 0f;
    float vertValue = 0f;
    float speed = 0f;

    public Vector3 TargetMovementVector(float speed, float smoothing = 6f)
    {
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cam.right;

        float forwardTarget = Input.GetAxisRaw(playerInput.verticalAxis);
        float rightTarget = Input.GetAxisRaw(playerInput.horizontalAxis);

        turnValue = Mathf.Lerp(turnValue, rightTarget, Time.deltaTime * 6f);
        vertValue = Mathf.Lerp(vertValue, forwardTarget, Time.deltaTime * 6f);
        this.speed = Mathf.Lerp(this.speed, speed, Time.deltaTime * 4f);

        if (this.speed < 0.1f)
            this.speed = 0f;

        Vector3 targetVector = camForward * vertValue
            + camRight * turnValue;
        if (targetVector.magnitude > 1f)
            targetVector = targetVector.normalized;
        targetVector.y = 0f;
        targetVector *= this.speed;

        return targetVector;
    }

    public Vector3 RawTargetVector(float speed = 1f)
    {
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cam.right;

        Vector3 targetVector = camForward * Input.GetAxisRaw(playerInput.verticalAxis)
            + camRight * Input.GetAxisRaw(playerInput.horizontalAxis);
        if (targetVector.magnitude > 1f)
            targetVector.Normalize();
        targetVector.y = 0f;
        targetVector *= speed;

        return targetVector;
    }

    public bool adjustingRot = false;

    public void MoveGrounded(float speed, bool pushDown = true, float smoothing = 10f)
    {
        Vector3 targetVector = TargetMovementVector(speed);

        velocity.y = 0f; // So slerp is correct when pushDown is true

        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);

        bool turning = animState.IsName("IdleTurns") || animState.IsName("RunTurns");

        targetAngle = Vector3.SignedAngle(transform.forward, RawTargetVector(), Vector3.up);
        targetSpeed = UMath.GetHorizontalMag(RawTargetVector(speed));

        holdRotation = Mathf.Abs(targetAngle) > (animState.IsName("Idle") ? 80f : 170f) || turning;
        
        anim.SetFloat("SignedTargetAngle", targetAngle, turning ? 1000000f : 0, Time.deltaTime);
        anim.SetFloat("TargetAngle", Mathf.Abs(targetAngle), turning ? 1000000f : 0, Time.deltaTime);
        anim.SetFloat("TargetSpeed", targetSpeed);

        if (UMath.GetHorizontalMag(velocity) < 0.1f)
        {
            if (!adjustingRot && targetVector.magnitude > 0.1f && Mathf.Abs(targetAngle) > 5f/*5*/)
            {
                adjustingRot = true;
                velocity = Mathf.Abs(targetAngle) > 80f ? targetVector : transform.forward * 3f;
                
            }
            else if (UMath.GetHorizontalMag(targetVector) < 0.1f)
            {
                velocity = Vector3.zero;
            }
        }
        else if (Mathf.Abs(targetAngle) > 90f /*20*/)
        {
            adjustingRot = true;
        }

        if (turning)
            adjustingRot = false; // Stops turning code clashing with root motion

        if (!turning)
        {
            if (adjustingRot)
            {
                velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * smoothing);
                if (Vector3.Angle(velocity, targetVector) < 5f)
                {
                    adjustingRot = false;
                }
            }
            else
            {
                velocity = targetVector;
            }
        }

        if (!turning)
            anim.SetFloat("Speed", UMath.GetHorizontalMag(velocity), 0.1f, Time.deltaTime);
        else
            anim.SetFloat("Speed", UMath.GetHorizontalMag(targetVector), 0.1f, Time.deltaTime);
        anim.SetFloat("Right", 0f);

        if (pushDown)
            velocity.y = -gravity;  // so charControl is grounded consistently
    }

    public void MoveStrafeGround(float speed, bool pushDown = true, float smoothing = 10f)
    {
        Vector3 targetVector = TargetMovementVector(speed);

        velocity.y = 0f; // So slerp is correct when pushDown is true

        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);

        targetAngle = Vector3.SignedAngle(transform.forward, RawTargetVector(), Vector3.up);
        targetSpeed = UMath.GetHorizontalMag(RawTargetVector(speed));

        bool backwards = Vector3.Angle(cam.forward, targetVector) > 90f;

        if (backwards)
        {
            targetVector = -targetVector;
        }

        Vector3 camForward = new Vector3(cam.forward.x, 0f, cam.forward.z);
        anim.SetFloat("SignedTargetAngle", Vector3.SignedAngle(cam.forward, targetVector, Vector3.up));

        anim.SetFloat("TargetSpeed", targetSpeed);

        if (UMath.GetHorizontalMag(velocity) < 0.1f)
        {
            if (UMath.GetHorizontalMag(targetVector) < 0.1f)
            {
                velocity = Vector3.zero;
            }
        }

        velocity = targetVector;

        float speedCalc = UMath.GetHorizontalMag(velocity) * (backwards ? -1f : 1f);
        anim.SetFloat("Speed", speedCalc, 0.1f, Time.deltaTime);
        

        if (pushDown)
            velocity.y = -gravity;  // so charControl is grounded consistently
    }

    public void MoveFree(float speed, float smoothing = 16f, float maxTurnAngle = 20f)
    {
        Vector3 targetVector = cam.forward * Input.GetAxisRaw("Vertical")
            + cam.right * Input.GetAxisRaw("Horizontal");
        if (targetVector.magnitude > 1.0f)
            targetVector = targetVector.normalized;

        if (velocity.magnitude < 0.1f && targetVector.magnitude > 0f)
            velocity = transform.forward * 0.1f;  // Player will rotate smoothly from idle

        if (Vector3.Angle(velocity.normalized, targetVector) > maxTurnAngle)
        {
            Vector3 direction = Vector3.Cross(velocity.normalized, targetVector);
            targetVector = Quaternion.AngleAxis(maxTurnAngle, direction) * velocity.normalized;
        }

        targetVector *= speed;

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * smoothing);

        anim.SetFloat("Speed", velocity.magnitude);
        anim.SetFloat("TargetSpeed", targetVector.magnitude);
    }

    public void MoveInDirection(float speed, Vector3 dir, float smoothing = 8f, float maxTurnAngle = 24f)
    {
        Vector3 targetVector = dir;

        if (velocity.magnitude < 0.1f && targetVector.magnitude > 0f)
            velocity = transform.forward * 0.1f;  // Player will rotate smoothly from idle

        if (Vector3.Angle(velocity.normalized, targetVector) > maxTurnAngle)
        {
            Vector3 direction = Vector3.Cross(velocity.normalized, targetVector);
            targetVector = Quaternion.AngleAxis(maxTurnAngle, direction) * velocity.normalized;
        }

        targetVector *= speed;

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * smoothing);

        anim.SetFloat("Speed", velocity.magnitude);
        anim.SetFloat("TargetSpeed", targetVector.magnitude);
    }

    public void RotateToCamera()
    {
        if (UMath.GetHorizontalMag(velocity) > 0.1f)
        {
            Quaternion target = Quaternion.LookRotation(cam.transform.forward, Vector3.up);
            target = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
            transform.rotation = target;
        }
    }

    public void RotateToVelocityGround(float smoothing = 0f)
    {
        // if stops Lara returning to the default rotation when idle
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
        // if stops Lara returning to the default rotation when idle
        if (velocity.magnitude > 0.1f)
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

    public void MinimizeCollider(float size = 0f)
    {
        charControl.radius = size;
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

    public Quaternion WaistRotation
    {
        get { return WaistRotation; }
        set { waistRotation = value; }
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

    public bool ForceWaistRotation
    {
        get { return forceWaistRotation; }
        set { forceWaistRotation = value; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }

    public RaycastHit GroundHit
    {
        get { return groundHit; }
    }

}
