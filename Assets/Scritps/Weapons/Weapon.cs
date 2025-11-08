using UnityEngine;
using Photon.Pun;

public class Weapon : MonoBehaviourPun
{
    [Header("Weapon Stats")]
    public float damage = 25f;
    public float fireRate = 0.15f;
    public float range = 100f;
    public int bulletsPerShot = 1;
    public float spread = 1f;

    [Header("References")]
    public Transform firePoint;
    public LayerMask hitLayers = -1;

    [Header("Visual")]
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;

    private float nextFireTime = 0f;

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (firePoint == null) return;

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        for (int i = 0; i < bulletsPerShot; i++)
        {
            FireRaycast();
        }
    }

    void FireRaycast()
    {
        Vector3 direction = firePoint.forward;

        if (spread > 0)
        {
            direction += new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                Random.Range(-spread, spread)
            ) * 0.01f;
            direction.Normalize();
        }

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, direction, out hit, range, hitLayers))
        {
            Health targetHealth = hit.collider.GetComponent<Health>();

            if (targetHealth == null)
            {
                targetHealth = hit.collider.GetComponentInParent<Health>();
            }

            if (targetHealth != null)
            {
                PhotonView targetPV = targetHealth.GetComponent<PhotonView>();

                if (targetPV != null)
                {
                    if (targetPV.ViewID == photonView.ViewID) return;

                    string myTeam = "";
                    string targetTeam = "";

                    if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
                    {
                        myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
                    }

                    if (targetPV.Owner.CustomProperties.ContainsKey("Team"))
                    {
                        targetTeam = (string)targetPV.Owner.CustomProperties["Team"];
                    }

                    if (myTeam == targetTeam && !string.IsNullOrEmpty(myTeam)) return;

                    targetHealth.TakeDamage(damage, photonView);
                }
            }

            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }
        }
    }
}