using UnityEngine;

public class Ring : MonoBehaviour {

    public bool moving;
    private float tim = 2f;
    private int direction = 0;
    private int type = 0;
    private float rotDirection = 0;
    private float curRot = 0;
    private float tarRot = 0;
    private float curPos = 0;
    private float tarPos = 0;
    void Update()
    {
        tim += Time.deltaTime;
        if (Mathf.Abs(curPos - tarPos) > 0.05f)
        {
            int childCount = transform.childCount;
            
            transform.Translate(0f, Mathf.Min(Mathf.Abs(7f * Time.deltaTime), Mathf.Abs(tarPos - curPos)) * Mathf.Sign(tarPos - curPos), 0f);
            curPos += Mathf.Min(Mathf.Abs(7f * Time.deltaTime), Mathf.Abs(tarPos - curPos)) * Mathf.Sign(tarPos - curPos);
            transform.Rotate(0f, 180f * Time.deltaTime, 0f);
            curRot += 180f * Time.deltaTime;
            curRot %= 360;
            rotDirection = 1;
            float radius = 0;
            switch (type)
            {
                case 1: radius = Mathf.Max(0.1f, (1 - tim * 3.5f)) * 2f; break;
                case 2: radius = Mathf.Min(2f, tim * 3.5f); break ;
                default: break;
            }
            for (int i = 0; i < childCount; i++)
            {
                transform.GetChild(i).localPosition = new Vector3(radius * Mathf.Sin(2 * i * Mathf.PI / childCount), 0f, radius * Mathf.Cos(2 * i * Mathf.PI / childCount));
            }
        }
        else
        {
            direction = 0;
            if(rotDirection == 0) moving = false;
        }
        if(Mathf.Abs(3.6f * rotDirection) > Mathf.Abs(tarRot - curRot))
        {
            transform.Rotate(0f, tarRot-curRot, 0f);
            curRot = tarRot;
            rotDirection = 0;
            if(direction == 0) moving = false;
            return;
        }
        transform.Rotate(0f, -3.6f * rotDirection, 0f);
        curRot -= 3.6f * rotDirection;
        curRot %= 360;
    }

    public void Move(int pDirection, int pType)
    {
        direction = pDirection;
        tarPos += pDirection * 4f;
        type = pType;
        tim = 0f;
        moving = true;
    }

    public void RotateOneStep(int pRotRirection)
    {
        int childCount = transform.childCount;
        rotDirection = pRotRirection;
        tarRot -= rotDirection * 360f / childCount;
        tarRot %= 360;
        moving = true;
        tim = 0f;
    }
}
