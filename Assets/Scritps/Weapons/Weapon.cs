using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string weaponName = "Pistol";
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 0.5f;
    public int bulletsPerShot = 1;
    public float spread = 0f;

    public Transform firePoint;
    public LayerMask hitLayers = -1;
    private float nextFireTime = 0f;

    void Update()
    {
        if (!GetComponentInParent<PhotonView>().IsMine)
            return;
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        Debug.Log(weaponName + " shoot");
        for (int i = 0; i < bulletsPerShot; i++)
        {
            ShootRaycast();
        }
    }

    void ShootRaycast()
    {
        Vector3 shootDirection = firePoint.forward;
        if (spread > 0)
        {
            shootDirection += Random.insideUnitSphere * spread * 0.01f;
            shootDirection.Normalize();
        }

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, shootDirection, out hit, range, hitLayers))
        {

            Health target = hit.collider.GetComponent<Health>();

            if (target == null)
            {
                target = hit.collider.GetComponentInParent<Health>();
            }

            if (target != null)
            {
                if(target.isPlayer)
                {
                    PhotonView targetPhotonView = target.GetComponent<PhotonView>();
                    if (targetPhotonView != null)
                    {
                        targetPhotonView.RPC("ApplyDamage", targetPhotonView.Owner, damage);
                    }
                    else
                    {
                        target.TakeDamage(damage);
                    }
                }
                else
                {
                    target.TakeDamage(damage);
                }
            }
        }

        Debug.DrawRay(firePoint.position, shootDirection * range, Color.red, 0.5f);
    }
}