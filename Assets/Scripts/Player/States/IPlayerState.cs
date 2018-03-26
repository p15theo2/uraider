using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerState
{
    void OnEnter(PlayerController player);
    void OnExit(PlayerController player);
    void Update(PlayerController player);
}
