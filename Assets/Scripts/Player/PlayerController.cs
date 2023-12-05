using System;
using System.Collections;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Rolling Physics")]
    public float Speed = 50;
    public float MaxSpeed = 80;
    public float JumpForce = 8;
    public float Gravity = 100;

    [Header("States")]
    public bool Grounded       = true;
    public bool Rolling        = false;
    public bool RollingStorage = false;

    [Header("Turning Physics")]
    public bool DynamicTurningSpeeds = true;
    public float turnSpeedFactor;
    [HideInInspector] public float movementX;
    [HideInInspector] public float movementY;

    [Header("Debug Stats")]
    [HideInInspector] public Rigidbody rb;
    public Transform Camera;
    public Animator  animator;
    private Timers   timings;
    private Spear    spear;
    public Vector3   PlayerVelocity;
    public float     ForwardVelocityMagnitude;
    public float     turnSpeed;
    public Vector3   CamF;
    public Vector3   CamR;
    public float     Blend;
    private int      decimals = 2;

    [Header("Stored Values")]
    public int Speed1    = 200; //Walk     Speed
    public int Speed2    = 150; //Roll     Speed
    public int MaxSpeed1 = 20;  //Walk Max Speed
    public int MaxSpeed2 = 200; //Roll Max Speed


    void Awake()
    {
        timings  = GetComponent<Timers>();
        rb       = GetComponent<Rigidbody>();
        spear    = GetComponent<Spear>();
        animator = GetComponentInChildren<Animator>();
        Camera   = GameObject.Find("Camera").transform;

        rb.useGravity = false;
    }

    void FixedUpdate()
    {   
        #region PerFrame Calculations
            CamF = Camera.forward;
            CamR = Camera.right;
            CamF.y = 0;
            CamR.y = 0;
            CamF = CamF.normalized;
            CamR = CamR.normalized;

            // Calculate the Forward velocity magnitude
            Vector3 ForwardVelocity = Vector3.Project(rb.velocity, CamF);
            ForwardVelocityMagnitude = ForwardVelocity.magnitude;
            ForwardVelocityMagnitude = (float)System.Math.Round(ForwardVelocityMagnitude, decimals);

            // Calculate the Forward Angle
            float targetAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;
            Quaternion toRotation = Quaternion.Euler(0f, targetAngle, 0f);
        #endregion
        //**********************************
        #region Animations
            // if(Grounded && !Rolling && !RollingStorage && !animator.GetCurrentAnimatorStateInfo(0).IsName("Land"))
            // {
            //     if (rb.velocity.magnitude > 0.1)      animator.Play("Walk");
            //     else if (rb.velocity.magnitude < 0.1) animator.Play("Idle");
            // }
            
            //Speed Modifier
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk Idle - BlendTree"))
            {
                animator.speed = rb.velocity.magnitude/20;
                animator.speed = math.clamp(animator.speed, 0, 1);
                Blend = rb.velocity.magnitude/20;
                animator.SetFloat("Blend", Blend, 0.1f, Time.deltaTime);
            }
            if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Walk Idle - BlendTree") || Blend < 0.01) animator.speed = 1;

            if(!Grounded)
            {
                if(rb.velocity.y > 0 && !Rolling && !RollingStorage) animator.Play("Jump");
                else if(rb.velocity.y < 0 && !Rolling && !RollingStorage) animator.Play("Fall");
            }
        #endregion
        //**********************************


        //Walking
        if(!Rolling && Grounded && timings.RollStorageTimer == 0)
        {
            if(RollingStorage)
            {
                Speed = Speed1;
                if(!spear.Aiming && !spear.AimStorage) MaxSpeed = MaxSpeed1; //Not Aiming
                else                                   MaxSpeed = MaxSpeed1; //Aiming
                RollingStorage = false;
            }
            rb.freezeRotation = true;
            if(rb.velocity.magnitude > 0.5) transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.25f);
        }

        //Roll Storage Period
        if(!Rolling && RollingStorage && !Grounded)
        {
            rb.freezeRotation = true;
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.06f);
        }

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


        Vector3 movement = (CamF * movementY + CamR * movementX).normalized;
        turnSpeed = turnSpeedFactor * ForwardVelocityMagnitude;
        rb.AddForce(movement * Speed + CamR * movementX * turnSpeed);

        //Debug.DrawLine(transform.position, Camera.forward*100, Color.red, 0f);
        //Debug.DrawLine(transform.position, Camera.up*100, Color.green, 0f);

        #region Debug Stats
            PlayerVelocity   = rb.velocity;
            PlayerVelocity.x = (float)Math.Round(PlayerVelocity.x, decimals);
            PlayerVelocity.y = (float)Math.Round(PlayerVelocity.y, decimals);
            PlayerVelocity.z = (float)Math.Round(PlayerVelocity.z, decimals);
            turnSpeed        = (float)Math.Round(turnSpeed, decimals);
            Blend            = (float)Math.Round(Blend, decimals);
        #endregion
    }

    public void OnMove(InputAction.CallbackContext movementValue)
    {  
        Vector2 inputVector = movementValue.ReadValue<Vector2>();
        movementX = inputVector.x;
        movementY = inputVector.y;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.started && Grounded)
        {
            rb.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
            //JumpAudio.Play();
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        //Start Rolling
        if(context.started)
        {
            Rolling           = true;
            RollingStorage    = true;
            rb.freezeRotation = false;
            Speed             = Speed2;
            MaxSpeed          = MaxSpeed2;
            if(spear.Aiming)
            {
                spear.Aiming     = false;
                spear.AimStorage = true;
            }
            if(rb.velocity.magnitude > 0.1) //Dive Roll
            {
                animator.Play("Dive Down");
            }
            if (rb.velocity.magnitude > 0.1 && Grounded) //Get Down Roll
            {
                animator.Play("Get Down");
            }
            if(!Grounded && Rolling && RollingStorage) //Flip Over
            {
                animator.Play("Flip Over");
            }
        }
        //Stop Rolling
        else if(context.canceled)
        {
            Rolling = false;
            if(spear.AimStorage)
            {
                spear.Aiming     = true;
                spear.AimStorage = false;
            }
            if(Grounded)
            {
                if(rb.velocity.magnitude > 0.1) //Bounce Up
                {
                    rb.AddForce(Vector3.up * 12, ForceMode.VelocityChange);
                    timings.RollStorageTimer = 0.1f;
                    animator.Play("Flip Upright");
                }
                else //Get Up
                {
                    animator.Play("Get Up");
                }
            }
            else if(!Grounded && !Rolling && RollingStorage) //Flip Upright
            {
                animator.Play("Flip Upright");
            }
        }
    }

    public void SetGrounded(bool state) 
    {
        Grounded = state;
    }
}