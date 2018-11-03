using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medipack : InventoryItem
{
    public int healthIncrease = 20;

    public override void Use(PlayerController player)
    {
        player.Stats.Health += healthIncrease;
    }
}
