using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Code References")]
    private PlayerController playerController;
    private Timers           timings;

    private Rigidbody rb;
    private Animator  animator;

    [Header("Movement Physics")]
    public float Speed = 50;
    public float MaxSpeed = 80;
    public float Gravity = 100;

    [Header("Properties")]
    public float AgroStart;
    public float AgroEnd;
    public float AttackRange;

    [Header("States")]
    public bool Grounded = true;
    public bool Agro = false;

    [Header("Turning Physics")]
    [HideInInspector] public float movementX;
    [HideInInspector] public float movementY;
    [HideInInspector] public Vector3 movement;

    [Header("Debug Stats")]
    public float      Blend;
    private int       decimals = 2;

    void Awake()
    {
        timings          = GetComponent<Timers>();
        playerController = FindObjectOfType<PlayerController>();

        rb       = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        rb.useGravity = false;
    }


    void FixedUpdate()
    {
        #region PerFrame Calculations
            // Forward Rotation Stuff
            float targetAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;
            Quaternion toRotation = Quaternion.Euler(0f, targetAngle, 0f);
        #endregion
        //**********************************
        #region Extra Physics Stuff
            //Extra Gravity
            rb.AddForce(Physics.gravity * Gravity /10);

            //max speed
            if (rb.velocity.magnitude > MaxSpeed)
            {
                // Get the velocity direction
                Vector3 newVelocity = rb.velocity;
                newVelocity.y = 0f;
                newVelocity = Vector3.ClampMagnitude(newVelocity, MaxSpeed);
                newVelocity.y = rb.velocity.y;
                rb.velocity = newVelocity;
            }
        #endregion
        //**********************************

        if(timings.EnemyAttackDuration == 0 )
        {
            if (Agro) 
            {
                if(Vector3.Distance(transform.position, playerController.gameObject.transform.position) > AgroEnd) LoseAgro();

                else
                {
                    movement = (playerController.transform.position - transform.position).normalized;

                    //Attack
                    if(Vector3.Distance(transform.position, playerController.gameObject.transform.position) < AttackRange) EnemyAttack();
                }
            }
            else if(Vector3.Distance(transform.position, playerController.gameObject.transform.position) < AgroStart) StartAgro();
        }

        if(rb.velocity.magnitude > 0.5 && Grounded) transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.25f);
        rb.AddForce(movement * Speed);

        #region Debug Stats
            Blend = (float)Math.Round(Blend, decimals);
        #endregion
    }
    


    public void StartAgro()
    {
        Agro = true;
    }

    public void LoseAgro()
    {
        Agro = false;
        movement = new Vector3(0,0,0);
    }

    public void EnemyAttack()
    {
        timings.EnemyAttackDuration = 1;
        movement = new Vector3(0,0,0);
        playerController.TakeDamage(this.gameObject);
    }

}