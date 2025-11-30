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

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioSource audioSource;

    private float nextFireTime = 0f;
    private Camera mainCamera;

    void Start()
    {
        if (photonView.IsMine)
        {
            mainCamera = GetComponentInParent<PlayerCamera>()?.mainCam;
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }
    }

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

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        photonView.RPC("PlayShootEffectsRPC", RpcTarget.Others);

        for (int i = 0; i < bulletsPerShot; i++)
        {
            FireRaycast();
        }
    }

    [PunRPC]
    void PlayShootEffectsRPC()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    void FireRaycast()
    {
        Ray ray;
        if (mainCamera != null)
        {
            ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            ray = new Ray(firePoint.position, firePoint.forward);
        }

        Vector3 direction = ray.direction;

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
        if (Physics.Raycast(ray.origin, direction, out hit, range, hitLayers))
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

                    if (CrosshairSystem.Instance != null)
                    {
                        CrosshairSystem.Instance.ShowHitmarker();
                    }
                }
            }

            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * range);
        }
    }
}