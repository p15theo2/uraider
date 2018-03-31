using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeDetector
{
    private static LedgeDetector instance;

    private float minDepth = 0.1f;
    private float minHeight = 0.1f;
    private float maxAngle = 30f; // TODO: Implement use
    private int rayCount = 12;

    private Vector3 grabPoint;
    private Vector3 direction;

    private LedgeDetector()
    {

    }

    public bool FindLedgeJump(Vector3 start, Vector3 dir, float maxDistance, float maxHeight)
    {
        float deltaHeight = maxHeight / rayCount;
        for (int i = 0; i < rayCount; i++)
        {
            Vector3 offset = (Vector3.up * deltaHeight * i);
            if (FindLedgeAtPoint(start + offset, dir, maxDistance, deltaHeight))
                return true;
        }
        return false;
    }

    public bool CanClimbUp(Vector3 start, Vector3 dir)
    {
        if (FindLedgeJump(start + 2.12f * Vector3.up, dir, 1.0f, 1.8f))
        {
            return false;
        }
        return true;
    }

    public bool FindLedgeAtPoint(Vector3 start, Vector3 dir, float maxDistance, float deltaHeight)
    {
        RaycastHit hHit;
        Debug.DrawRay(start, dir * maxDistance, Color.red, 1.0f);
        if (Physics.Raycast(start, dir, out hHit, maxDistance))
        {
            RaycastHit vHit;
            start = new Vector3(hHit.point.x + (minDepth * dir.x), 
                start.y + deltaHeight, 
                hHit.point.z + (minDepth * dir.z));
            Debug.DrawRay(start, Vector3.down * deltaHeight, Color.red, 1.0f);
            if (Physics.Raycast(start, Vector3.down, out vHit, deltaHeight))
            {
                grabPoint = new Vector3(hHit.point.x, vHit.point.y, hHit.point.z);
                direction = -hHit.normal;
                return true;
            }
        }
        return false;
    }

    public bool FindPlatformInfront(Vector3 start, Vector3 dir, float maxHeight, float depth = 0.76f)
    {
        RaycastHit vHit;
        Vector3 vStart = start + (Vector3.up * 2f) + (dir * depth);
        Debug.DrawRay(vStart, Vector3.down * (maxHeight - 0.01f), Color.red, 1f);
        if (Physics.Raycast(vStart, Vector3.down, out vHit, (maxHeight - 0.01f)))
        {
            RaycastHit hHit;
            start = new Vector3(start.x, vHit.point.y - 0.01f, start.z);
            Debug.DrawRay(start, dir * depth, Color.red, 1f);
            if (Physics.Raycast(start, dir, out hHit, depth))
            {
                grabPoint = new Vector3(hHit.point.x, vHit.point.y, hHit.point.z);
                direction = -hHit.normal;
                return true;
            }
        }
        return false;
    }

    public float MinDepth
    {
        get { return minDepth; }
        set { minDepth = value; }
    }

    public float MinHeight
    {
        get { return minHeight; }
        set { minHeight = value; }
    }

    public float MaxAngle
    {
        get { return maxAngle; }
        set { maxAngle = value; }
    }

    public int RayCount
    {
        get { return rayCount; }
        set { rayCount = value; }
    }

    public Vector3 GrabPoint
    {
        get { return grabPoint; }
    }

    public Vector3 Direction
    {
        get { return direction; }
    }

    public static LedgeDetector Instance
    {
        get
        {
            if (instance == null)
                instance = new LedgeDetector();
            return instance;
        }
    }
}
