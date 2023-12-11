using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    private PlayerController playerController;
    public float Volume;
    public AudioSource StepSFX;
    public AudioClip[] steps;

    void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    void FootstepSfx()
    {
        Volume = playerController.Blend/1.5f;
        StepSFX.volume = Volume;
        StepSFX.clip = steps[Random.Range(0,14)];
        StepSFX.Play();
    }

    public void RecoverOrDie()
    {
        if(playerController.Health > 0)
        {
            Debug.Log("Recover");
        }
        else if(playerController.Health <= 0)
        {
            Debug.Log("Darkness for You... and Death for your People");
        }
    }
}
