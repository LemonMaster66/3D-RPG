using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    public PlayerController playerController;
    public Spear spear;
    public GroundCheck groundCheck; 

    public int VolumeDivide = 60;
    public float FallingVelocity;

    [Header("Main Player Physics Sounds")]
    public AudioSource Roll;
    public AudioSource JumpAudio;
    public AudioSource LandAudio;
    
    [Header("Spear")]
    public AudioSource SpearSFX;
    public AudioClip[] ThrowSpear;
    public AudioClip[] RecallSpear;
    public AudioClip   StopReeling;
    public AudioSource ReelSpeer;

    [Header("SpearImpale")]
    public AudioSource Impale;
    public AudioClip[] ImpaleGround;
    public AudioClip[] ImpaleEnemy;
    public AudioClip CatchSpear;

    [Header("Death")]
    public AudioSource Death;


    void FixedUpdate()
    {
        if(playerController.Grounded)
        {
            if(playerController.Rolling)
            {
                Roll.volume = playerController.rb.velocity.magnitude/100;
            }
        }
        else
        {
            FallingVelocity = playerController.rb.velocity.y*-1 / VolumeDivide;
            LandAudio.volume = FallingVelocity;
            Roll.volume = 0;
        }
    }
}
