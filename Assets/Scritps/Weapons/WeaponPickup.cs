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

        // Asegurarse de que el visual esté activo al inicio
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
        // Solo el Master Client procesa pickups (evita duplicados)
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!isAvailable)
            return;

        // Verificar si es un jugador
        PhotonView playerPhotonView = other.GetComponent<PhotonView>();
        if (playerPhotonView == null)
            return;

        // Solo procesar si es el jugador local que colisionó
        if (!playerPhotonView.IsMine)
            return;

        WeaponInventory playerInventory = other.GetComponent<WeaponInventory>();
        if (playerInventory == null)
            return;

        // Verificar si el jugador ya tiene esta arma
        if (playerInventory.HasWeapon(weaponData))
        {
            Debug.Log("El jugador ya tiene esta arma");
            return;
        }

        // Dar el arma directamente al jugador local
        playerInventory.AddWeapon(weaponData);

        // Desactivar pickup para todos
        photonView.RPC("DisablePickup", RpcTarget.AllBuffered);

        // Programar respawn si está activado
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
            // Si no hay visualModel, desactivar el objeto completo
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