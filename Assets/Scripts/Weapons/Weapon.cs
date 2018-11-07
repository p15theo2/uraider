using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject bullet;

    public int ammo = 0;
    public int clips = 0;
    public int ammoPerClip = 0;
    public float bulletDistance = 100f;

    private GameObject flash;
    private WeaponSFX sfx;
    private Vector3 target;

    private void Start()
    {
        flash = transform.Find("Flash").gameObject;
        sfx = GetComponent<WeaponSFX>();
    }

    public void Fire()
    {
        Vector3 origin = flash.transform.position;
        Vector3 direction = ((target + Vector3.up * 0.8f) - origin).normalized;
        GameObject firedBullet = Instantiate(bullet, origin, transform.rotation);

        firedBullet.transform.position = origin + (direction * 0.2f);
        Destroy(firedBullet, 2f);

        StartCoroutine(DoFlash());
        sfx.PlayBang();

        RaycastHit hit;
        Debug.DrawRay(origin, direction * bulletDistance, Color.red);
        if (Physics.Raycast(origin, direction, out hit, bulletDistance))
        {
            if (hit.transform.gameObject.CompareTag("Enemy"))
            {
                EnemyController enemy = hit.collider.gameObject.GetComponent<EnemyController>();
                enemy.Health -= 10;
                Debug.Log("Health now: " + enemy.Health);
            }
        }
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
