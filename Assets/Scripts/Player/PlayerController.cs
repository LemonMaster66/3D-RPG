using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Properties")]
    public int  Health;
    private int MaxHealth = 5;

    [Header("Movement Physics")]
    public float Speed      = 50;
    public float MaxSpeed   = 80;
    public float JumpForce  = 8;
    public float Gravity    = 100;

    [Header("States")]
    public bool Grounded       = true;
    public bool Rolling        = false;
    public bool RollingStorage = false;
    public bool Stunned        = false;
    public bool Dead           = false;

    [Header("Turning Physics")]
    public bool DynamicTurningSpeeds = true;
    public float turnSpeedFactor;
    [HideInInspector] public Vector3 movement;
    [HideInInspector] public float   movementX;
    [HideInInspector] public float   movementY;

    [Header("Debug Stats")]
    public Vector3     PlayerVelocity;
    private float      ForwardVelocityMagnitude;
    public float       VelocityMagnitudeXZ;
    private float      turnSpeed;
    public float       Blend;
    public int         Speed1    = 200; //Walk     Speed
    public int         Speed2    = 150; //Roll     Speed
    public int         MaxSpeed1 = 20;  //Walk Max Speed
    public int         MaxSpeed2 = 200; //Roll Max Speed
    [HideInInspector]  public Vector3 CamF;
    [HideInInspector]  public Vector3 CamR;

    #region Script / Component Reference
        private PlayerSFX        playerSFX;
        private AnimationEvents  animationEvents;
        private Timers           timings;
        private Spear            spear;

        [HideInInspector] public Rigidbody  rb;
        [HideInInspector] public Transform  Camera;
        [HideInInspector] private Animator  animator;
    #endregion


    void Awake()
    {
        //Assign Components
        timings         = GetComponent<Timers>();
        rb              = GetComponent<Rigidbody>();
        spear           = GetComponent<Spear>();
        animator        = GetComponentInChildren<Animator>();
        animationEvents = FindObjectOfType<AnimationEvents>();
        playerSFX       = FindObjectOfType<PlayerSFX>();
        Camera          = GameObject.Find("Camera").transform;

        //Component Values
        rb.useGravity = false;

        //Property Values
        Health = MaxHealth;
    }

    void FixedUpdate()
    {
        #region Physics Stuff
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
            ForwardVelocityMagnitude = (float)Math.Round(ForwardVelocityMagnitude, 2);

            VelocityMagnitudeXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

            // Calculate the Forward Angle
            float targetAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;
            Quaternion toRotation = Quaternion.Euler(0f, targetAngle, 0f);

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

            if(!Grounded && !Stunned && !Dead)
            {
                if(rb.velocity.y > 0 && !Rolling && !RollingStorage) animator.Play("Jump");
                else if(rb.velocity.y < 0 && !Rolling && !RollingStorage) animator.Play("Fall");
            }
        #endregion
        //**********************************

        if(Stunned || Dead) return;

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
            
            if(VelocityMagnitudeXZ > 0.5) transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.25f);
        }

        //Roll Storage Period
        if(!Rolling && RollingStorage && !Grounded)
        {
            rb.freezeRotation = true;
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.06f);
        }

        movement = (CamF * movementY + CamR * movementX).normalized;
        turnSpeed = turnSpeedFactor * ForwardVelocityMagnitude;
        rb.AddForce(movement * Speed + CamR * movementX * turnSpeed);

        #region Debug Stats
            PlayerVelocity      = rb.velocity;
            PlayerVelocity.x    = (float)Math.Round(PlayerVelocity.x, 2);
            PlayerVelocity.y    = (float)Math.Round(PlayerVelocity.y, 2);
            PlayerVelocity.z    = (float)Math.Round(PlayerVelocity.z, 2);
            VelocityMagnitudeXZ = (float)Math.Round(VelocityMagnitudeXZ, 2);
            turnSpeed           = (float)Math.Round(turnSpeed, 2);
            Blend               = (float)Math.Round(Blend, 2);
        #endregion
    }

    //***********************************************************************
    //***********************************************************************
    //Movement Functions
    public void OnMove(InputAction.CallbackContext movementValue)
    {  
        Vector2 inputVector = movementValue.ReadValue<Vector2>();
        movementX = inputVector.x;
        movementY = inputVector.y;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.started && Grounded && !Stunned && !Dead)
        {
            rb.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
            playerSFX.JumpAudio.Play();
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if(Stunned || Dead) return;

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
                animator.Play("Spear Aim Out Mix");
                spear.BlendTarget = 0;
                spear.BlendSmoothness = 0.5f;
            }
            if(spear.HoldingAim && spear.Collided) spear.Reeling = false;

            if(VelocityMagnitudeXZ > 0.1 && Grounded) animator.Play("Dive Down");
            else if (VelocityMagnitudeXZ < 0.1 && Grounded) animator.Play("Get Down");
            else if(!Grounded && Rolling && RollingStorage) animator.Play("Flip Over");
        }
        //Stop Rolling
        else if(context.canceled)
        {
            Rolling = false;
            if(spear.AimStorage)
            {
                spear.Aiming     = true;
                spear.AimStorage = false;
                animator.Play("Spear Aim In Mix");
                spear.BlendTarget = 1;
                spear.BlendSmoothness = 0.2f;
            }
            if(spear.Reeling) spear.Reeling = false;
            if(Grounded)
            {
                if(VelocityMagnitudeXZ > 0.1) //Bounce Up
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


    //***********************************************************************
    //***********************************************************************
    //Other Functions
    public void TakeDamage(GameObject Attacker)
    {
        Health--;

        Stunned = true;
        Rolling = false;
        RollingStorage = false;

        rb.freezeRotation = true;

        Vector3 directionToTarget = transform.position - Attacker.transform.position;
        rb.velocity = directionToTarget.normalized;
        rb.velocity = new Vector3(rb.velocity.x*20, rb.velocity.y*20+10, rb.velocity.z*20);
        transform.rotation = Quaternion.LookRotation(directionToTarget*-1, Vector3.up);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        if(!Dead) animator.Play("Take Damage");
        
        if(spear.Aiming)
        {
            spear.Aiming = false;
            spear.AimStorage = false;
            spear.HoldingAim = false;
            animator.Play("Spear Aim Out Mix");
        }
    }
}