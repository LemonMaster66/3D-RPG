using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceType : MonoBehaviour
{
    [Header("Surface Type")]
    public Surface surface;
    public enum Surface
    {
        Default,
        Grass,
        Stone,
        Dirt
    }
}