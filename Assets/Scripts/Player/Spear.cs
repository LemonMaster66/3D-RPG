using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spear : MonoBehaviour
{
    private PlayerController playerController;
    private CinemachineFreeLook cinemachineFreeLook;

    public GameObject SpearPrefab;
    public GameObject SpearObject;
    //public SpearCollision spearCollision;

    public float ThrowForce = 100;
    public float Gravity = 9.8f;
    public float CameraZoom = 0;

    [Header("States")]
    public bool FoundSpear   = true;
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
        playerController = FindObjectOfType<PlayerController>();
        cinemachineFreeLook = FindObjectOfType<CinemachineFreeLook>();
    }

    void FixedUpdate()
    {
        if(Aiming)
        {
            //Walking
            if(!playerController.Rolling)
            {
                cinemachineFreeLook.m_Lens.FieldOfView = Mathf.SmoothDamp(cinemachineFreeLook.m_Lens.FieldOfView, 30, ref CameraZoom, 0.2f);
                playerController.MaxSpeed = playerController.MaxSpeed1-10;
            }
        }
        else
        {
            cinemachineFreeLook.m_Lens.FieldOfView = Mathf.SmoothDamp(cinemachineFreeLook.m_Lens.FieldOfView, 40, ref CameraZoom, 0.05f);
            cinemachineFreeLook.m_Lens.LensShift.y = 1;
        }

        CameraZoom = (float)Math.Round(CameraZoom, 2);
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
}
