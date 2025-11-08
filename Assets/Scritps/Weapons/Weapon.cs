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

    [Header("Debug")]
    public bool showDebugRays = true;

    private float nextFireTime = 0f;

    void Update()
    {
        if (!photonView.IsMine) return;

        // TECLA T para test rápido
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("=== TEST RAYCAST MANUAL ===");
            Debug.Log("FirePoint position: " + firePoint.position);
            Debug.Log("FirePoint forward: " + firePoint.forward);
            Debug.Log("HitLayers value: " + hitLayers.value);

            RaycastHit hit;
            bool didHit = Physics.Raycast(firePoint.position, firePoint.forward, out hit, range, hitLayers);

            Debug.Log("Raycast result: " + didHit);

            if (didHit)
            {
                Debug.Log("HIT OBJECT: " + hit.collider.name);
                Debug.Log("Hit Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
                Debug.Log("Hit Distance: " + hit.distance);
                Debug.Log("Tiene Health: " + (hit.collider.GetComponent<Health>() != null));
                Debug.Log("Tiene Health en Parent: " + (hit.collider.GetComponentInParent<Health>() != null));
                Debug.Log("Tiene PhotonView: " + (hit.collider.GetComponent<PhotonView>() != null));
                Debug.Log("Tiene PhotonView en Parent: " + (hit.collider.GetComponentInParent<PhotonView>() != null));
            }
            else
            {
                Debug.LogWarning("No impactó nada en " + range + " metros");
            }

            // Test sin layer mask
            bool didHitNoMask = Physics.Raycast(firePoint.position, firePoint.forward, out hit, range);
            Debug.Log("Raycast SIN layer mask: " + didHitNoMask);
            if (didHitNoMask)
            {
                Debug.Log("Sin mask impactó: " + hit.collider.name);
            }
        }

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogError("¡No hay FirePoint asignado en " + gameObject.name + "!");
            return;
        }

        Debug.Log("=== DISPARO ===");
        Debug.Log("Shooter: " + photonView.Owner.NickName);

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

        Debug.Log("Raycast desde: " + firePoint.position + " hacia: " + direction);

        // Raycast
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, direction, out hit, range, hitLayers))
        {
            Debug.Log(" IMPACTO en: " + hit.collider.name + " | Distancia: " + hit.distance + " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));

            // Buscar componente de salud en el objeto impactado
            Health targetHealth = hit.collider.GetComponent<Health>();

            // Si no está en el collider, buscar en el padre
            if (targetHealth == null)
            {
                targetHealth = hit.collider.GetComponentInParent<Health>();
                if (targetHealth != null)
                {
                    Debug.Log("Health encontrado en padre: " + targetHealth.gameObject.name);
                }
            }
            else
            {
                Debug.Log("Health encontrado en collider: " + targetHealth.gameObject.name);
            }

            // Si encontramos un objetivo con salud
            if (targetHealth != null)
            {
                PhotonView targetPV = targetHealth.GetComponent<PhotonView>();

                if (targetPV != null)
                {
                    Debug.Log("Target PhotonView ID: " + targetPV.ViewID + " | Owner: " + targetPV.Owner.NickName);
                    Debug.Log("My PhotonView ID: " + photonView.ViewID + " | Owner: " + photonView.Owner.NickName);

                    // NO dispararse a sí mismo
                    if (targetPV.ViewID == photonView.ViewID)
                    {
                        Debug.LogWarning("Intentaste dispararte a ti mismo");
                        return;
                    }

                    // Verificar que no sea del mismo equipo (friendly fire)
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

                    Debug.Log("Mi equipo: " + myTeam + " | Equipo del objetivo: " + targetTeam);

                    if (myTeam == targetTeam && !string.IsNullOrEmpty(myTeam))
                    {
                        Debug.LogWarning("Friendly fire bloqueado - Mismo equipo: " + myTeam);
                        return;
                    }

                    // Aplicar daño
                    Debug.Log(">>> APLICANDO " + damage + " de daño a " + targetPV.Owner.NickName);
                    targetHealth.TakeDamage(damage, photonView);
                }
                else
                {
                    Debug.LogWarning("El objetivo tiene Health pero no PhotonView");
                }
            }
            else
            {
                Debug.LogWarning("Impacto en objeto sin Health: " + hit.collider.name);
            }

            // Efecto de impacto
            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }

            // Línea de debug
            if (showDebugRays)
            {
                Debug.DrawLine(firePoint.position, hit.point, Color.red, 2f);
            }
        }
        else
        {
            Debug.LogWarning(" NO IMPACTÓ NADA");
            // No impactó nada
            if (showDebugRays)
            {
                Debug.DrawRay(firePoint.position, direction * range, Color.yellow, 2f);
            }
        }
    }
}