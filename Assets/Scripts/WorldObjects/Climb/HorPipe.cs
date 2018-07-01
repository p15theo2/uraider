using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class HorPipe : MonoBehaviour
{
    public static HorPipe CUR_PIPE;

    public Vector3 point1;
    public Vector3 point2;
}
