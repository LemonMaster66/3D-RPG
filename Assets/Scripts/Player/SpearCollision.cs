using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearCollision : MonoBehaviour
{
    public Spear      spear;
    public bool       collided = false;
    public GameObject CollidedObject;

    void Awake()
    {
        spear = FindObjectOfType<Spear>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(collided || spear.Recalling) return;
        CollidedObject = other.gameObject;

        if (other.tag == "Enemy") spear.CollideEnemy();
        else                      spear.CollideGround();
    }
}
