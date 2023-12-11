using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timers : MonoBehaviour
{
    public float RollStorageTimer;
    public float EnemyAttackDuration;
    public float StunnedDuration;

    private void RollStorageTimerFunction()
    {
        RollStorageTimer -= Time.deltaTime;
        if(RollStorageTimer <= 0f) RollStorageTimer = 0;
        return;
    }
    private void EnemyAttackDurationFunction()
    {
        EnemyAttackDuration -= Time.deltaTime;
        if(EnemyAttackDuration <= 0f) EnemyAttackDuration = 0;
        return;
    }
    private void StunnedDurationFunction()
    {
        StunnedDuration -= Time.deltaTime;
        if(StunnedDuration <= 0f) StunnedDuration = 0;
        return;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(RollStorageTimer > 0)     RollStorageTimerFunction();
        if(EnemyAttackDuration > 0)  EnemyAttackDurationFunction();
        if(StunnedDuration > 0)  StunnedDurationFunction();
    }
}
