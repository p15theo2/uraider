using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSelector : MonoBehaviour
{
    public EventSystem eventSystem;
    public GameObject objectToSelect;

    private bool isSelecting = false;

	void Start ()
    {
		
	}
	
	void Update ()
    {
		if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.3f && !isSelecting)
        {
            eventSystem.SetSelectedGameObject(objectToSelect);
            isSelecting = true;
        }
	}

    void OnDisable()
    {
        isSelecting = false;
    }
}
