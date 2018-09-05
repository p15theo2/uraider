using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEngaged : StateBase<EnemyController>
{
    public override void OnEnter(EnemyController enemy)
    {
        enemy.Anim.SetBool("isEngaged", true);
        //enemy.NavAgent.enabled = false;
    }

    public override void OnExit(EnemyController enemy)
    {
        enemy.Anim.SetBool("isEngaged", false);
    }

    public override void Update(EnemyController enemy)
    {
        enemy.Anim.SetFloat("Speed", enemy.NavAgent.velocity.magnitude);
        //Debug.Log("Speed: " + enemy.NavAgent.speed);

        float distance = Vector3.Distance(enemy.Target.transform.position, enemy.transform.position);

        if (enemy.Health <= 0)
        {
            enemy.Anim.SetBool("isDead", true);

            enemy.NavAgent.enabled = false;

            return;
        }
        else if (Mathf.Abs(distance) > enemy.maxAimDistance)
        {
            enemy.StateMachine.GoToState<AIChase>();
        }

        Vector3 direction = enemy.Target.transform.position - enemy.transform.position;
        enemy.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        enemy.Anim.SetLookAtPosition(enemy.Target.transform.position + Vector3.up * 1.75f);
        enemy.Anim.SetLookAtWeight(1f);
    }
}
