using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    public PlayerController playerController;
    public Animator animator;

    public int VolumeDivide = 60;
    public float FallingVelocity;

    [Header("Main Player Physics Sounds")]
    public AudioSource WalkingAudio;
    public AudioClip[] WalkType;
    public AudioSource RollingAudio;
    public AudioClip[] RollType;

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
        //Rolling Audio
        if (playerController.Grounded)
        {
            RollingAudio.volume = playerController.rb.velocity.magnitude/VolumeDivide;
        }
        else
        {
            RollingAudio.volume=0;

            FallingVelocity = playerController.rb.velocity.y*-1 / VolumeDivide;
            LandAudio.volume = FallingVelocity;
            LandAudio.Play();
        }
    }
}
