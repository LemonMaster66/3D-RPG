using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Transform Camera;
    public GameObject StandingModel;
    public GameObject RollingModel;
    
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
    private float movementX;
    private float movementY;

    [Header("Boulder Audio")]
    public AudioSource RollingAudio;
    public AudioSource JumpAudio;
    public AudioSource LandAudio;
    public int VolumeDivide = 60;

    [Header("Debug Stats")]
    private Rigidbody rb;
    private Timers timings;
    private Spear spear;
    public Vector3 PlayerVelocity;
    public float ForwardVelocityMagnitude;
    public float turnSpeed;
    public float FallingVelocity;

    [Header("Stored Values")]
    public int Speed1    = 200; //Walk     Speed
    public int Speed2    = 150; //Roll     Speed
    public int MaxSpeed1 = 20;  //Walk Max Speed
    public int MaxSpeed2 = 200; //Roll Max Speed


    void Awake()
    {
        timings = GetComponent<Timers>();
        rb      = GetComponent<Rigidbody>();
        spear   = GetComponent<Spear>();
        Camera  = GameObject.Find("Camera").transform;

        rb.useGravity = false;
    }

    void FixedUpdate()
    {   
        #region Debug Stats
            int decimals = 2;
            PlayerVelocity = rb.velocity;
            PlayerVelocity.x = (float)Math.Round(PlayerVelocity.x, decimals);
            PlayerVelocity.y = (float)Math.Round(PlayerVelocity.y, decimals);
            PlayerVelocity.z = (float)Math.Round(PlayerVelocity.z, decimals);
            turnSpeed = (float)Math.Round(turnSpeed, decimals);
        #endregion
        //**********************************
        #region Sfx
            //Rolling Audio
            if (Grounded)
            {
                RollingAudio.volume = rb.velocity.magnitude/VolumeDivide;
            }
            else
            {
                RollingAudio.volume=0;

                FallingVelocity = rb.velocity.y*-1 / VolumeDivide;
                LandAudio.volume = FallingVelocity;
                LandAudio.Play();
            }
        #endregion
        //**********************************
        #region PerFrame Calculations
            Vector3 CamF = Camera.forward;
            Vector3 CamR = Camera.right;
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
            if(rb.velocity != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.25f);
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
            JumpAudio.Play();
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
            StandingModel.SetActive(false);
            RollingModel.SetActive(true);
            if(spear.Aiming)
            {
                spear.Aiming     = false;
                spear.AimStorage = true;
            }
            if(Grounded)
            {
                if(rb.velocity.magnitude > 0.1) //Dive Roll
                {
                    Debug.Log("Dive Roll");
                }
                else //Get Down Roll
                {
                    Debug.Log("Get Down Roll");
                }
            }
            else if(!Grounded && Rolling && RollingStorage) //Flip Over
            {
                Debug.Log("Flip Over");
            }
        }
        //Stop Rolling
        else if(context.canceled)
        {
            Rolling = false;
            StandingModel.SetActive(true);
            RollingModel.SetActive(false);
            if(spear.AimStorage)
            {
                spear.Aiming     = true;
                spear.AimStorage = false;
            }
            if(Grounded)
            {
                if(rb.velocity.magnitude > 0.1) //Bounce Up
                {
                    Debug.Log("Bounce Up");
                    rb.AddForce(Vector3.up * 12, ForceMode.VelocityChange);
                    timings.RollStorageTimer = 0.1f;
                }
                else //Get Up
                {
                    Debug.Log("Get Up");
                }
            }
            else if(!Grounded && !Rolling && RollingStorage) //Flip Upright
            {
                Debug.Log("Flip Upright");
            }
        }
    }

    public void SetGrounded(bool state) 
    {
        Grounded = state;
    }
}