using Photon.Pun;
using UnityEngine;

public class WeaponController : MonoBehaviourPun
{
    [Header("References")]
    public Transform firePoint;
    public LayerMask hitLayers = -1;

    [Header("Settings")]
    public bool showLogs = true;

    private WeaponData currentWeaponData;
    private float nextFireTime = 0f;

    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (currentWeaponData == null)
            return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + currentWeaponData.fireRate;
        }
    }

    public void SetWeapon(WeaponData weaponData)
    {
        currentWeaponData = weaponData;

        if (showLogs && weaponData != null)
        {
            Debug.Log("Arma configurada: " + weaponData.weaponName);
        }
    }

    void Fire()
    {
        if (currentWeaponData == null || firePoint == null)
            return;

        if (showLogs)
        {
            Debug.Log(currentWeaponData.weaponName + " disparado");
        }

        for (int i = 0; i < currentWeaponData.bulletsPerShot; i++)
        {
            ShootRaycast();
        }

    }

    void ShootRaycast()
    {
        Vector3 shootDirection = firePoint.forward;

        if (currentWeaponData.spread > 0)
        {
            shootDirection += Random.insideUnitSphere * currentWeaponData.spread * 0.01f;
            shootDirection.Normalize();
        }

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, shootDirection, out hit, currentWeaponData.range, hitLayers))
        {
            Health target = hit.collider.GetComponent<Health>();

            if (target == null)
            {
                target = hit.collider.GetComponentInParent<Health>();
            }

            if (target != null)
            {
                if (target.isPlayer)
                {
                    PhotonView targetPhotonView = target.GetComponent<PhotonView>();
                    if (targetPhotonView != null)
                    {
                        targetPhotonView.RPC("ApplyDamage", targetPhotonView.Owner, currentWeaponData.damage);
                    }
                    else
                    {
                        target.TakeDamage(currentWeaponData.damage);
                    }
                }
                else
                {
                    target.TakeDamage(currentWeaponData.damage);
                }
            }

            CreateImpactEffect(hit.point, hit.normal);
        }

        Debug.DrawRay(firePoint.position, shootDirection * currentWeaponData.range, Color.red, 0.5f);
    }

    void CreateImpactEffect(Vector3 position, Vector3 normal)
    {
        //De momento nada
    }

    public WeaponData GetCurrentWeaponData()
    {
        return currentWeaponData;
    }
}