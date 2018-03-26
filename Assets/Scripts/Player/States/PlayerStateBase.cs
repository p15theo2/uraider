using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerStateBase<T> : IPlayerState 
    where T : PlayerStateBase<T>, new()
{
    private static readonly T instance = new T();

    public static T Instance
    {
        get { return instance; }
    }

    public abstract void OnEnter(PlayerController player);
    public abstract void OnExit(PlayerController player);
    public abstract void Update(PlayerController player);
}
