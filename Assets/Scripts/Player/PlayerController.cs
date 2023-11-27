using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Transform Camera;
    
    [Header("Rolling Physics")]
    public float speed = 50;
    public float maxSpeed = 80;
    public float JumpForce = 8;
    public float Gravity = 100;

    public bool Grounded       = true;
    public bool Rolling        = false;
    public bool RollingStorage = true;

    private Rigidbody rb;
    private Timers timings;

    [Header("Turning Physics")]
    [Tooltip("Turning Speeds Increase With Forward Momentum (Recomended)")]
    public bool DynamicTurningSpeeds = true;

    private float movementX;
    private float movementY;

    public float turnSpeedFactor;


    [Header("Boulder Audio")]
    public AudioSource RollingAudio;
    public AudioSource JumpAudio;
    public AudioSource LandAudio;

    private float XVolume;
    private float ZVolume;

    [Tooltip("How Gradualy the Rolling Volume Increases *60 Default*")]
    public int VolumeDivide = 60;

    [Header("Debug Stats")]
    public Vector3 PlayerVelocity;
    public float ForwardVelocityMagnitude;
    public float turnSpeed;
    public float FallingVelocity;


    void Awake()
    {
        timings = GetComponent<Timers>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Camera = GameObject.Find("Camera").transform;
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
                speed = 200;
                maxSpeed = 20;
                RollingStorage = false;
            }
            rb.freezeRotation = true;
            if(rb.velocity != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 0.25f);
            //transform.rotation = Quaternion.Euler(0,transform.rotation.y,0);
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
        if (rb.velocity.magnitude > maxSpeed)
        {
            // Get the velocity direction
            Vector3 newVelocity = rb.velocity;
            newVelocity.y = 0f;
            newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
            newVelocity.y = rb.velocity.y;
            rb.velocity = newVelocity;
        }


        Vector3 movement = (CamF * movementY + CamR * movementX).normalized;
        turnSpeed = turnSpeedFactor * ForwardVelocityMagnitude;
        rb.AddForce(movement * speed + CamR * movementX * turnSpeed);
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
            Rolling = true;
            RollingStorage = true;
            rb.freezeRotation = false;
            if(Grounded)
            {
                speed = 150;
                maxSpeed = 200;
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