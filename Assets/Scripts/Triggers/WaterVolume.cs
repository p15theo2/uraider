using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterVolume : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playControl = other.gameObject.GetComponent<PlayerController>();
            playControl.StateMachine.GoToState<Swimming>();
        }
        else if (other.CompareTag("MainCamera"))
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = Color.blue;
            RenderSettings.fogDensity = 0.1f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            RenderSettings.fog = false;
        }
        else if (other.CompareTag("Player"))
        {
            PlayerController playControl = other.gameObject.GetComponent<PlayerController>();
            playControl.StateMachine.GoToState<Locomotion>();
        }
    }
}
