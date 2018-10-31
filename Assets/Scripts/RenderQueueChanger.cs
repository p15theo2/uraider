using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderQueueChanger : MonoBehaviour
{
    private void Update()
    {
        GetComponent<MeshRenderer>().material.renderQueue = (int)RenderQueue.GeometryLast;
    }
}
