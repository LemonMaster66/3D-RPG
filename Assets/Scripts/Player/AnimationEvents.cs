using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    private PlayerController playerController;
    private Enemy enemy;
    public float Volume;

    [Header("SFX")]
    public AudioSource StepSFX;
    public AudioClip[] steps;

    void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        enemy = GetComponentInParent<Enemy>();
    }

    void FootstepSfx()
    {
        Volume = playerController.Blend/1.5f;
        StepSFX.volume = Volume;
        StepSFX.clip = steps[Random.Range(0,14)];
        StepSFX.Play();
    }

    void EnemyFootstepSfx()
    {
        Volume = enemy.Blend;
        StepSFX.volume = Volume;
        StepSFX.clip = steps[Random.Range(0,4)];
        StepSFX.Play();
    }
}
