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
        if(!playerController.Rolling)
        {
            playerController.Blend = 0;
            playerSFX.LandAudio.Play();
            animator.Play("Land");
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