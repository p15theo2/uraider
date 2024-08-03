using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverTypeOne : LeverTarget {
	public override void Action()
	{
		gameObject.GetComponent<Rigidbody>().useGravity = true;
	}
}
