using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timers : MonoBehaviour
{
    public float RollStorageTimer;

    private void RollStorageTimerFunction()
    {
        RollStorageTimer -= Time.deltaTime;
        if(RollStorageTimer <= 0f) RollStorageTimer = 0;
        return;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(RollStorageTimer > 0) RollStorageTimerFunction();
    }
}
