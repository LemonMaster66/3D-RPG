using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
    public int Health;
    private int MaxHealth = 5;
    public float AgroStart;
    public float AgroEnd;
    public float AttackRange;

    [Header("States")]
    public bool Grounded = true;
    public bool Agro = false;
    public bool Dead = false;
    

    [Header("Turning Physics")]
    [HideInInspector] public float movementX;
    [HideInInspector] public float movementY;
    [HideInInspector] public Vector3 movement;

    [Header("Debug Stats")]
    public float      Blend;
    public float      VelocityMagnitudeXZ;
    private int       decimals = 2;

    [Header("SFX")]
    public AudioSource AttackSfx;
    public AudioClip[] AttackClip;


    void Awake()
    {
        timings          = GetComponent<Timers>();
        playerController = FindObjectOfType<PlayerController>();

        rb       = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        //Component Values
        rb.useGravity = false;

        //Property Values
        Health = MaxHealth;
    }


    void FixedUpdate()
    {
        #region PerFrame Calculations
            // Forward Rotation Stuff
            float targetAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;
            Quaternion toRotation = Quaternion.Euler(0f, targetAngle, 0f);

            VelocityMagnitudeXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
        #endregion
        //**********************************
        #region Animations
            //Speed Modifier
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk Idle - BlendTree"))
            {
                animator.speed = VelocityMagnitudeXZ/20;
                animator.speed = Math.Clamp(animator.speed, 0, 1);
                Blend = VelocityMagnitudeXZ/20;
                animator.SetFloat("Blend", Blend, 0.1f, Time.deltaTime);
            }
            if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Walk Idle - BlendTree") || Blend < 0.01) animator.speed = 1;

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
        

        if(timings.EnemyAttackDuration == 0 && !Dead)
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

        if(rb.velocity.magnitude > 0.5 && Grounded && !Dead) transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.25f);
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
        playerController.TakeDamage(this.gameObject);
        
        movement = new Vector3(0,0,0);

        Vector3 directionToTarget = transform.position - playerController.transform.position;
        transform.rotation = Quaternion.LookRotation(directionToTarget*-1, Vector3.up);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        animator.Play("Attack");
        AttackSfx.clip = AttackClip[UnityEngine.Random.Range(0,3)];
        AttackSfx.pitch = 1 + UnityEngine.Random.Range(-0.2f,0.2f);
        AttackSfx.Play();
    }

    public void Die()
    {
        Health--;
        Dead = true;

        rb.freezeRotation = true;

        movement = new Vector3(0,0,0);
        Vector3 directionToTarget = transform.position - playerController.transform.position;
        rb.velocity = directionToTarget.normalized;
        rb.velocity = new Vector3(rb.velocity.x*100, rb.velocity.y*20+5, rb.velocity.z*100);

        animator.Play("Dead");
    }

}
