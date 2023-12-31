using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spear : MonoBehaviour
{
    private PlayerController    playerController;
    private CinemachineFreeLook cinemachineFreeLook;
    private CinemachineComposer cinemachineComposer;
    private SpearCollision      spearCollision;
    private PlayerSFX           playerSFX;

    public GameObject SpearPrefab;
    public GameObject SpearObject;

    public float ThrowForce = 75;
    public float Gravity = 9.8f;

    private float CameraZoom = 0;
    private float CameraScreenY = 0;
    public float AnimationBlend = 0;
    public float BlendTarget = 0;
    public float BlendSmoothness;

    [Header("States")]
    public bool FoundSpear = true;
    public bool HasSpear   = true;
    public bool Aiming     = false;
    public bool AimStorage = false;
    public bool HoldingAim = false;
    public bool Throwing   = false;
    public bool Recalling  = false;
    public bool Reeling    = false;
    public bool Collided   = false;

    private Rigidbody rb;
    private Animator animator;
    public Vector3 throwDirection;
    public GameObject collidedObject;

    public AudioSource SpearWind;
    public AudioSource Impale;

    void Start()
    {
        playerController    = FindObjectOfType<PlayerController>();
        animator            = GetComponentInChildren<Animator>();
        cinemachineFreeLook = FindObjectOfType<CinemachineFreeLook>();
        cinemachineComposer = FindObjectOfType<CinemachineComposer>();
        playerSFX           = FindObjectOfType<PlayerSFX>();
    }

    void FixedUpdate()
    {
        if(Aiming && !playerController.Dead)
        {
            //Walking
            if(!playerController.Rolling)
            {
                cinemachineFreeLook.m_Lens.FieldOfView = Mathf.SmoothDamp(cinemachineFreeLook.m_Lens.FieldOfView, 30, ref CameraZoom, 0.2f);
                cinemachineComposer.m_ScreenY          = Mathf.SmoothDamp(cinemachineComposer.m_ScreenY, 1.0f, ref CameraScreenY, 0.2f);
                playerController.MaxSpeed = playerController.MaxSpeed1-10;
            }
        }
        else if(!Aiming && !playerController.Dead)
        {
            cinemachineFreeLook.m_Lens.FieldOfView = Mathf.SmoothDamp(cinemachineFreeLook.m_Lens.FieldOfView, 45, ref CameraZoom, 0.05f);
            cinemachineComposer.m_ScreenY          = Mathf.SmoothDamp(cinemachineComposer.m_ScreenY, 0.75f, ref CameraScreenY, 0.05f);
            if(!playerController.Rolling && playerController.MaxSpeed == playerController.MaxSpeed1-10) playerController.MaxSpeed = playerController.MaxSpeed1;
        }

        if(animator.GetLayerWeight(1) != BlendTarget)
        {
            animator.SetLayerWeight(1, Mathf.SmoothDamp(animator.GetLayerWeight(1), BlendTarget, ref AnimationBlend, BlendSmoothness));
        }

        if(Throwing)  ThrowingCalculations();
        if(Recalling) RecallingCalculations();
        if(Reeling)   ReelingCalculations();

        AnimationBlend = (float)Math.Round(AnimationBlend, 2);
    }


    //****************************************************************
    //****************************************************************
    //****************************************************************

    public void AimInSpear()
    {
        Aiming = true;
        animator.Play("Spear Aim In Mix");
        BlendTarget = 1;
        BlendSmoothness = 0.2f;
    }

    public void ThrowSpear()
    {
        SpearObject = Instantiate(SpearPrefab, transform.position + new Vector3(0, 2.5f, 0), Quaternion.identity);
        spearCollision = SpearObject.GetComponentInChildren<SpearCollision>();
        rb = SpearObject.GetComponent<Rigidbody>();

        //Thrown   = true;
        Throwing = true;
        Aiming   = false;
        HasSpear = false;
        rb.drag = 0;
        rb.angularDrag = 1;
        
        rb.velocity = playerController.Camera.forward*ThrowForce;
        Quaternion rotation = Quaternion.LookRotation(rb.velocity);
        rotation *= Quaternion.Euler(90,90,0);
        SpearObject.transform.rotation = rotation;

        animator.Play("Spear Throw");
        animator.SetLayerWeight(1, 1);
        BlendTarget = 0;
        BlendSmoothness = 0.5f;

        SpearWind = SpearObject.GetComponentInChildren<SpearParts>().SpearWind;
        Impale = SpearObject.GetComponentInChildren<SpearParts>().Impale;
        SpearWind.Play();
        playerSFX.SpearSFX.clip = playerSFX.ThrowSpear[UnityEngine.Random.Range(0,2)];
        playerSFX.SpearSFX.Play();
    }

    public void RecallSpear()
    {
        Recalling = true;
        Throwing = false;
        rb.drag = 3;
        rb.angularDrag = 3;
        rb.isKinematic = false;
        rb.freezeRotation = false;
        Gravity = 0;

        animator.Play("Spear Recall");
        BlendTarget = 1;
        BlendSmoothness = 0.05f;

        SpearWind.Play();
    }

    public void ResetSpear()
    {
        Destroy(SpearObject);
        HasSpear  = true;
        Recalling = false;
        Collided  = false;
        Throwing  = false;
        Reeling   = false;
        if(HoldingAim && !playerController.Rolling)
        {
            Aiming = true;
            animator.Play("Spear Aim In Mix");
            BlendTarget = 1;
            BlendSmoothness = 0.3f;
        }
        else if(HoldingAim && playerController.Rolling) AimStorage = true;
        else
        {
            animator.Play("Spear Catch");
            animator.SetLayerWeight(1, 1);
            BlendTarget = 0;
            BlendSmoothness = 0.5f;

            playerSFX.Impale.clip = playerSFX.CatchSpear;
            playerSFX.Impale.pitch = 1 + UnityEngine.Random.Range(-0.2f,0.2f);
            playerSFX.Impale.Play();
        }
    }

    public void StartReeling()
    {
        Reeling = true;
        playerSFX.ReelSpeer.Play();
    }
    public void StopReeling()
    {
        Reeling = false;
        playerSFX.ReelSpeer.Stop();
        playerSFX.SpearSFX.clip = playerSFX.StopReeling;
        playerSFX.SpearSFX.Play();
    }

    //****************************************************************
    //Per Frame Stuff

    public void ThrowingCalculations()
    {
        Quaternion rotation = Quaternion.LookRotation(rb.velocity);
        rotation *= Quaternion.Euler(0,90,0);
        SpearObject.transform.rotation = rotation;
        rb.AddForce(Physics.gravity * Gravity/30);
    }

    public void RecallingCalculations()
    {
        if (Vector3.Distance(transform.position, SpearObject.transform.position) < 5) ResetSpear();

        rb.velocity += new Vector3 ( (SpearObject.transform.position.x - transform.position.x) * (ThrowForce/150) *-1 ,
                                     (SpearObject.transform.position.y - transform.position.y-2.5f) * (ThrowForce/150) *-1 , 
                                     (SpearObject.transform.position.z - transform.position.z) * (ThrowForce/150) *-1 );
        Quaternion rotation = Quaternion.LookRotation(rb.velocity);
        rotation *= Quaternion.Euler(-90,-90,0);
        SpearObject.transform.rotation = rotation;
    }

    public void ReelingCalculations()
    {
        if (Vector3.Distance(transform.position, SpearObject.transform.position) < 5) ResetSpear();
        //Debug.DrawLine(transform.position, transform.position - SpearObject.transform.position, Color.red, 0); 

        playerController.rb.velocity += new Vector3 ( (transform.position.x - SpearObject.transform.position.x) / 30 *-1 ,
                                                      (transform.position.y - SpearObject.transform.position.y) / 30 *-1 , 
                                                      (transform.position.z - SpearObject.transform.position.z) / 30 *-1 );
    }


    //****************************************************************
    //Input Stuff


    public void OnThrow(InputAction.CallbackContext context)
    {
        if(!FoundSpear || playerController.Stunned || playerController.Dead) return;
        if(context.started) //Press / Hold Button
        {
            HoldingAim = true;
            if(!playerController.Rolling && HasSpear) AimInSpear();                      //Aim Spear
            else if(!playerController.Rolling && !HasSpear) RecallSpear();               //Recall Spear
            else if(playerController.Rolling && HasSpear) AimStorage = true;             //Store Spear Aim
            else if(playerController.Rolling && !HasSpear && Collided) StartReeling();    //Reel In
        }
        else if(context.canceled && HoldingAim) //Release Button
        {
            HoldingAim = false;
            if(!playerController.Rolling && HasSpear) ThrowSpear();                      //Throw Spear
            else if(playerController.Rolling && HasSpear) AimStorage = false;            //Unstore Spear Aim
            else if(playerController.Rolling && !HasSpear) StopReeling();                //Stop Reeling In
        }
    }

    //****************************************************************
    //Other

    public void CollideGround()
    {
        spearCollision.collided = true;
        rb.velocity = new Vector2(0,0);
        rb.freezeRotation = true;
        rb.isKinematic = true;

        Throwing = false;
        Collided = true;

        SpearWind.Stop();
        Impale.clip = playerSFX.ImpaleGround[UnityEngine.Random.Range(0,4)];
        Impale.Play();
    }

    public void CollideEnemy()
    {
        SpearObject.transform.parent = spearCollision.CollidedObject.transform;
        spearCollision.collided = true;
        rb.velocity = new Vector2(0,0);
        rb.freezeRotation = true;
        rb.isKinematic = true;

        Throwing = false;
        Collided = true;

        spearCollision.CollidedObject.GetComponent<Enemy>().Die();

        SpearWind.Stop();
        Impale.clip = playerSFX.ImpaleEnemy[UnityEngine.Random.Range(0,3)];
        Impale.Play();
    }
}
