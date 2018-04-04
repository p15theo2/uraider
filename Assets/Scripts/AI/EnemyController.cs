﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int startHealth = 100;
    public float interpolationRate = 8f;

    private int health;

    private IAIState state;
    private GameObject target;
    private Animator anim;
    private CharacterController charControl;
    private Vector3 velocity;

    private void Start()
    {
        state = AIIdle.Instance;
        anim = GetComponent<Animator>();
        charControl = GetComponent<CharacterController>();
        target = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        state.Update(this);

        if (charControl.enabled)
            charControl.Move(velocity * Time.deltaTime);
    }

    public void MoveGrounded(float speed, bool pushDown = true)
    {
        Vector3 targetVector = (target.transform.position - transform.position).normalized;
        targetVector.y = 0f;
        targetVector *= speed;

        velocity.y = 0f; // So slerp is correct when pushDown is true

        if (velocity.magnitude < 0.1f && targetVector.magnitude > 0f)
            velocity = transform.forward * 0.1f;  // Player will rotate smoothly from idle

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * interpolationRate);

        anim.SetFloat("Speed", UMath.GetHorizontalMag(velocity));

        if (pushDown)
            velocity.y = -9f;  // so charControl is grounded consistently
    }

    public void RotateToVelocityGround(float smoothing = 0f)
    {
        if (UMath.GetHorizontalMag(velocity) > 0.1f)
        {
            Quaternion target = Quaternion.Euler(0.0f, Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg, 0.0f);
            if (smoothing == 0f)
                transform.rotation = target;
            else
                transform.rotation = Quaternion.Slerp(transform.rotation, target, smoothing * Time.deltaTime);
        }
    }

    public int Health
    {
        get { return health; }
        set { health = value; }
    }

    public IAIState State
    {
        get { return state; }
        set {
            state.OnExit(this);
            state = value;
            state.OnEnter(this);
        }
    }

    public Animator Anim
    {
        get { return anim; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }

    public GameObject Target
    {
        get { return target; }
        set { target = value; }
    }
}