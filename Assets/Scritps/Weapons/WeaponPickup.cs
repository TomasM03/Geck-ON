using Photon.Pun;
using UnityEngine;

public class WeaponPickup : MonoBehaviourPun
{
    [Header("Weapon Configuration")]
    public WeaponData weaponData;

    [Header("Pickup Settings")]
    public bool respawnAfterPickup = true;
    public float respawnTime = 30f;
    public bool rotatePickup = true;
    public float rotationSpeed = 50f;

    [Header("Visual Feedback")]
    public GameObject visualModel;

    private bool isAvailable = true;
    private Collider pickupCollider;

    void Start()
    {
        pickupCollider = GetComponent<Collider>();

        if (pickupCollider == null)
        {
            Debug.LogError("WeaponPickup necesita un Collider con 'Is Trigger' activado!");
        }

        if (weaponData == null)
        {
            Debug.LogError("No hay WeaponData asignado en " + gameObject.name);
        }

        if (visualModel != null)
        {
            visualModel.SetActive(true);
        }
    }

    void Update()
    {
        // Rotación visual del pickup
        if (rotatePickup && isAvailable)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!isAvailable)
            return;

        PhotonView playerPhotonView = other.GetComponent<PhotonView>();
        if (playerPhotonView == null)
            return;

        if (!playerPhotonView.IsMine)
            return;

        WeaponInventory playerInventory = other.GetComponent<WeaponInventory>();
        if (playerInventory == null)
            return;

        if (playerInventory.HasWeapon(weaponData))
        {
            Debug.Log("El jugador ya tiene esta arma");
            return;
        }

        playerInventory.AddWeapon(weaponData);

        photonView.RPC("DisablePickup", RpcTarget.AllBuffered);

        if (respawnAfterPickup)
        {
            Invoke("RespawnPickup", respawnTime);
        }
    }

    [PunRPC]
    void DisablePickup()
    {
        isAvailable = false;

        if (pickupCollider != null)
            pickupCollider.enabled = false;

        if (visualModel != null)
        {
            visualModel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void RespawnPickup()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("EnablePickup", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void EnablePickup()
    {
        isAvailable = true;

        if (pickupCollider != null)
            pickupCollider.enabled = true;

        if (visualModel != null)
        {
            visualModel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}