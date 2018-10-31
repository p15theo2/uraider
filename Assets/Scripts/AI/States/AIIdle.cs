using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIIdle : StateBase<EnemyController>
{
    public override void OnEnter(EnemyController enemy)
    {

    }

    public override void OnExit(EnemyController enemy)
    {

    }

    public override void Update(EnemyController enemy)
    {
        /*if (enemy.Health != enemy.startHealth)
            enemy.StateMachine.GoToState<AIChase>();*/
    }
}
