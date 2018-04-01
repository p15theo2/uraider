using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject bullet;

    private GameObject flash;
    private Vector3 target;

    private void Start()
    {
        flash = transform.Find("Flash").gameObject;
    }

    public void Fire()
    {
        Vector3 origin = flash.transform.position;
        GameObject firedBullet = Instantiate(bullet, origin, transform.rotation);

        firedBullet.transform.position = origin;
        Vector3 direction = (target - origin).normalized;
        Vector3 velocity = direction * 700f;

        Rigidbody rb = firedBullet.GetComponent<Rigidbody>();
        rb.velocity = velocity;

        Destroy(firedBullet, 2f);

        StartCoroutine(DoFlash());
    }

    private IEnumerator DoFlash()
    {
        float time = Time.time;
        flash.SetActive(true);
        while (Time.time - time < 0.05f)
        {
            yield return null;
        }
        flash.SetActive(false);
    }

    public Vector3 Target
    {
        get { return target; }
        set { target = value; }
    }
}
