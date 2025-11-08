using Photon.Pun;
using UnityEngine;

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
        // Efecto visual local
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Disparar múltiples balas si es necesario (escopeta)
        for (int i = 0; i < bulletsPerShot; i++)
        {
            FireRaycast();
        }
    }

    void FireRaycast()
    {
        // Dirección con spread
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

        // Raycast
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, direction, out hit, range, hitLayers))
        {
            // Buscar componente de salud
            Health targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth == null)
            {
                targetHealth = hit.collider.GetComponentInParent<Health>();
            }

            // Aplicar daño
            if (targetHealth != null)
            {
                PhotonView targetPV = targetHealth.GetComponent<PhotonView>();
                if (targetPV != null)
                {
                    targetHealth.TakeDamage(damage, photonView);
                }
            }

            // Efecto de impacto
            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }

            Debug.DrawLine(firePoint.position, hit.point, Color.red, 0.3f);
        }
    }
}
