using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEngaged : AIStateBase<AIEngaged>
{
    public override void OnEnter(EnemyController enemy)
    {
        
    }

    public override void OnExit(EnemyController enemy)
    {
        
    }

    public override void Update(EnemyController enemy)
    {
        enemy.MoveGrounded(3f);

        enemy.RotateToVelocityGround(10f);
    }
}
