using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEngaged : StateBase<EnemyController>
{
    public override void OnEnter(EnemyController enemy)
    {
        
    }

    public override void OnExit(EnemyController enemy)
    {
        
    }

    public override void Update(EnemyController enemy)
    {
        if (enemy.Health <= 0)
        {
            enemy.Anim.SetBool("isDead", true);

            enemy.NavAgent.enabled = false;

            return;
        }

        enemy.Anim.SetFloat("Speed", enemy.NavAgent.speed);
        enemy.NavAgent.SetDestination(enemy.Target.transform.position);
    }
}
