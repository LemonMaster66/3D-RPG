using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    public PlayerController playerController;
    public GroundCheck groundCheck; 

    public int VolumeDivide = 60;
    public float FallingVelocity;

    [Header("Main Player Physics Sounds")]
    public AudioSource Roll;
    public AudioSource Step;
    public AudioClip[] StepLR;

    public AudioSource JumpAudio;
    public AudioSource LandAudio;
    
    [Header("Spear")]
    public AudioSource Spear;
    public AudioClip[] ThrowSpear;
    public AudioClip[] RecallSpear;
    public AudioClip[] RellSpeer;

    [Header("SpearImpale")]
    public AudioSource Impale;
    public AudioClip[] ImpaleGround;
    public AudioClip[] ImpaleEnemy;
    public AudioClip[] CatchSpear;


    void FixedUpdate()
    {
        if(playerController.Grounded)
        {
            if(playerController.Rolling)
            {
                Roll.volume = playerController.rb.velocity.magnitude/VolumeDivide;
            }
        }
        else
        {
            FallingVelocity = playerController.rb.velocity.y*-1 / VolumeDivide;
            LandAudio.volume = FallingVelocity;
            Roll.volume = 0;
        }
    }

    public void StepSFX(int Leg)
    {
        Step.clip = StepLR[Leg];
        Step.Play();
    }

    // //Surface Type Beat
    // public void SurfaceFinder()
    // {
    //     if(groundCheck.GroundObject == null) return;
    //     SurfaceType surfaceType = groundCheck.GroundObject.GetComponent<SurfaceType>();
    //     if     (surfaceType.surface == SurfaceType.Surface.Default) SurfaceAudio = "Default";
    //     else if(surfaceType.surface == SurfaceType.Surface.Dirt)    SurfaceAudio = "Dirt";
    //     else if(surfaceType.surface == SurfaceType.Surface.Grass)   SurfaceAudio = "Grass";
    //     else if(surfaceType.surface == SurfaceType.Surface.Stone)   SurfaceAudio = "Stone";
    //     else SurfaceAudio = "Default";
    // }
}
