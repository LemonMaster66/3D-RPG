using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public PlayerController playerController;
    public PlayerSFX playerSFX;
    public GameObject GroundObject;
    public bool Grounded;

    public Surface surface;
    public enum Surface
    {
        Default,
        Grass,
        Stone,
        Dirt
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == playerController.gameObject) return;
        playerController.SetGrounded(true);
        GroundObject = other.gameObject;
        Grounded = true;
        playerSFX.LandAudio.Play();
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