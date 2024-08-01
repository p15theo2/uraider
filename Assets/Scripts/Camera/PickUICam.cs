using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUICam : MonoBehaviour {

	public Transform objectPoint;
	private GameObject item;
	// Use this for initialization
	void Start () {
		item = null;
	}
	
	// Update is called once per frame
	void Update () {
        if (item != null)
        {
            item.transform.Rotate(0f, 90f * Time.deltaTime, 0f);
        }
	}

	public void SetAndEnable(GameObject prefab)
	{
		item = Instantiate(prefab,objectPoint);
		item.transform.localPosition = Vector3.zero;
		item.layer = 5;
		item.transform.GetChild(0).gameObject.layer = 5;
        Destroy(item, 3f);
    }
}
