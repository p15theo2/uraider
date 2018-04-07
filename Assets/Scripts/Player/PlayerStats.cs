using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public int maxHealth = 100;

    public Canvas canvas;
    public RectTransform healthBar;

    private int health;

    private PlayerController player;

    private void Start()
    {
        health = maxHealth;
        player = GetComponent<PlayerController>();
    }

    public void ShowCanvas()
    {
        canvas.enabled = true;
    }

    public void HideCanvas()
    {
        canvas.enabled = false;
    }

    public int Health
    {
        get { return health; }
        set
        {
            health = value;

            if (health > maxHealth)
                health = maxHealth;
            else if (health <= 0)
            {
                health = 0;
                player.StateMachine.GoToState<Dead>();
            }

            healthBar.localScale = new Vector3((float)health / maxHealth, 1f, 1f);
        }
    }
}
