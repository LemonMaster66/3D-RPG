using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    private PlayerController playerController;

    public GameObject SpearPrefab;
    public GameObject SpearObject;
    //public SpearCollision spearCollision;

    public float ThrowForce = 100;
    public float Gravity = 9.8f;

    public bool hasSpear = true;
    public bool isThrowing = false;
    public bool isThrown = false;
    public bool isRecalling = false;
    public bool collided = false;

    private Rigidbody rb;
    public Vector3 throwDirection;
    public GameObject collidedObject;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }


    void Update()
    {
        if (hasSpear)
        {
            if (Input.GetMouseButtonDown(1) && !isThrown && !isRecalling)
            {
                //Play Throw Animation which contains the Throw Function
            }
            else if (Input.GetMouseButtonDown(1) && isThrown && !isRecalling)
            {
                //Recall();
            }
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * Gravity /10);
    }
}
