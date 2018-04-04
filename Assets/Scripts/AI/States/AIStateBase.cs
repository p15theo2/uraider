using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIStateBase<T> : IAIState
    where T : AIStateBase<T>, new()
{
    private static readonly T instance = new T();

    public static T Instance
    {
        get { return instance; }
    }

    public abstract void OnEnter(EnemyController enemy);

    public abstract void OnExit(EnemyController enemy);

    public abstract void Update(EnemyController enemy);
}
