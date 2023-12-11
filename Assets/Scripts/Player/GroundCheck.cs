using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public PlayerController playerController;
    public PlayerSFX playerSFX;
    public Animator animator;
    public GameObject GroundObject;
    public bool Grounded;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == playerController.gameObject) return;
        playerController.SetGrounded(true);
        GroundObject = other.gameObject;
        Grounded = true;
        if(!playerController.Rolling && !playerController.Stunned)
        {
            playerController.Blend = 0;
            playerSFX.LandAudio.Play();
            animator.Play("Land");
        }
        else if(playerController.Stunned && playerController.Health > 0)
        {
            playerController.Stunned = false;
            animator.Play("Land");
            Debug.Log("Ouch");
        }
        else if(playerController.Stunned && playerController.Health <= 0)
        {
            animator.Play("Land");
            Debug.Log("Dead");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerController.gameObject) return;
        playerController.SetGrounded(false);
        GroundObject = null;
        Grounded = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == playerController.gameObject) return;
        playerController.SetGrounded(true);
        GroundObject = other.gameObject;
        Grounded = true;
    }
}