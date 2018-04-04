using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIState
{
    void OnEnter(EnemyController enemy);
    void OnExit(EnemyController enemy);
    void Update(EnemyController enemy);
}
