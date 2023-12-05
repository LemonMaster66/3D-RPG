using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spear : MonoBehaviour
{
    private PlayerController playerController;
    private CinemachineFreeLook cinemachineFreeLook;
    private CinemachineComposer cinemachineComposer;
    private SpearCollision spearCollision;

    public GameObject SpearPrefab;
    public GameObject SpearObject;

    public float ThrowForce = 100;
    public float Gravity = 9.8f;

    private float CameraZoom = 0;
    private float CameraScreenY = 0;

    [Header("States")]
    public bool FoundSpear = true;
    public bool HasSpear   = true;
    public bool Aiming     = false;
    public bool AimStorage = false;
    public bool Thrown     = false;
    public bool Throwing   = false;
    public bool Recalling  = false;
    public bool Collided   = false;

    private Rigidbody rb;
    public Vector3 throwDirection;
    public GameObject collidedObject;

    void Start()
    {
        playerController    = FindObjectOfType<PlayerController>();
        cinemachineFreeLook = FindObjectOfType<CinemachineFreeLook>();
        cinemachineComposer = FindObjectOfType<CinemachineComposer>();
        spearCollision      = GetComponentInChildren<SpearCollision>();
    }

    void FixedUpdate()
    {
        if(Aiming)
        {
            //Walking
            if(!playerController.Rolling)
            {
                cinemachineFreeLook.m_Lens.FieldOfView = Mathf.SmoothDamp(cinemachineFreeLook.m_Lens.FieldOfView, 30, ref CameraZoom, 0.2f);
                cinemachineComposer.m_ScreenY          = Mathf.SmoothDamp(cinemachineComposer.m_ScreenY, 0.9f, ref CameraScreenY, 0.2f);
                playerController.MaxSpeed = playerController.MaxSpeed1-10;
            }
        }
        else
        {
            cinemachineFreeLook.m_Lens.FieldOfView = Mathf.SmoothDamp(cinemachineFreeLook.m_Lens.FieldOfView, 40, ref CameraZoom, 0.05f);
            cinemachineComposer.m_ScreenY          = Mathf.SmoothDamp(cinemachineComposer.m_ScreenY, 0.7f, ref CameraScreenY, 0.05f);
        }

        if(Throwing)
        {
            float targetAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;
            Quaternion toRotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
    }


    public void ThrowSpear()
    {
        SpearObject = Instantiate(SpearPrefab, transform.position, Quaternion.identity);
        rb = SpearObject.GetComponent<Rigidbody>();
        Thrown = true;
        Throwing = true;
        
        rb.velocity = playerController.Camera.forward*100;
        Quaternion rotation = Quaternion.LookRotation(rb.velocity);
        rotation *= Quaternion.Euler(90,0,0);
        SpearObject.transform.rotation = rotation;
    }


    public void OnThrow(InputAction.CallbackContext context)
    {
        if(!FoundSpear) return;
        if(context.started) //Press / Hold Button
        {
            if(!playerController.Rolling && HasSpear) //Aim Spear
            {
                Debug.Log("Aim Spear");
                playerController.MaxSpeed = playerController.MaxSpeed1-10;
                Aiming = true;
            }
            else if(!playerController.Rolling && !HasSpear) //Recall Spear*
            {
                Debug.Log("Recall Spear");
            }
            else if(playerController.Rolling && HasSpear) //Store Spear Aim
            {
                AimStorage = true;
            }
            else if(playerController.Rolling && !HasSpear) //Reel In*
            {
                Debug.Log("Reel In");
            }
        }
        else if(context.canceled) //Release Button
        {
            if(!playerController.Rolling && HasSpear) //Throw Spear*
            {
                Debug.Log("Throw Spear");
                playerController.MaxSpeed = playerController.MaxSpeed1;
                Aiming   = false;
                HasSpear = false;
                ThrowSpear();
            }
            else if(playerController.Rolling && HasSpear) //Unstore Spear Aim
            {
                AimStorage = false;
            }
            else if(playerController.Rolling && !HasSpear) //Stop Reeling In*
            {
                Debug.Log("Stop Reeling In");
            }
        }
    }

    public void CollideGround()
    {
        rb.isKinematic = true;
        rb.velocity = new Vector2(0,0);
        rb.freezeRotation = true;
        spearCollision.collided = true;
    }

    public void CollideEnemy()
    {
        SpearObject.transform.parent = spearCollision.CollidedObject.transform;
        rb.isKinematic = true;
        rb.velocity = new Vector2(0,0);
        rb.freezeRotation = true;
        spearCollision.collided = true;
    }
}
