using UnityEngine;
using Photon.Pun;

public class WeaponSwitcher : MonoBehaviourPun
{
    [Header("Weapons")]
    public GameObject pistol;    // Arrastra aquí tu pistola
    public GameObject shotgun;   // Arrastra aquí tu escopeta

    [Header("Weapon Stats")]
    [Tooltip("Pistola")]
    public float pistolDamage = 25f;
    public float pistolFireRate = 0.15f;
    public float pistolRange = 100f;
    public int pistolBulletsPerShot = 1;
    public float pistolSpread = 0.5f;

    [Tooltip("Escopeta")]
    public float shotgunDamage = 15f;
    public float shotgunFireRate = 0.8f;
    public float shotgunRange = 30f;
    public int shotgunBulletsPerShot = 8;  // Escopeta dispara 8 perdigones
    public float shotgunSpread = 5f;        // Más spread que la pistola

    private int currentWeaponIndex = 0; // 0 = pistola, 1 = escopeta
    private Weapon currentWeaponScript;

    void Start()
    {
        if (!photonView.IsMine) return;

        // Iniciar con la pistola
        SwitchToWeapon(0);
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Presionar 1 = Pistola
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToWeapon(0);
        }
        // Presionar 2 = Escopeta
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToWeapon(1);
        }
    }

    void SwitchToWeapon(int weaponIndex)
    {
        if (weaponIndex == currentWeaponIndex) return;

        currentWeaponIndex = weaponIndex;

        // Desactivar todas las armas
        if (pistol != null) pistol.SetActive(false);
        if (shotgun != null) shotgun.SetActive(false);

        // Activar el arma seleccionada y configurar sus stats
        if (weaponIndex == 0 && pistol != null)
        {
            pistol.SetActive(true);
            currentWeaponScript = pistol.GetComponent<Weapon>();

            if (currentWeaponScript != null)
            {
                currentWeaponScript.damage = pistolDamage;
                currentWeaponScript.fireRate = pistolFireRate;
                currentWeaponScript.range = pistolRange;
                currentWeaponScript.bulletsPerShot = pistolBulletsPerShot;
                currentWeaponScript.spread = pistolSpread;
            }

            Debug.Log("Cambiado a Pistola");
        }
        else if (weaponIndex == 1 && shotgun != null)
        {
            shotgun.SetActive(true);
            currentWeaponScript = shotgun.GetComponent<Weapon>();

            if (currentWeaponScript != null)
            {
                currentWeaponScript.damage = shotgunDamage;
                currentWeaponScript.fireRate = shotgunFireRate;
                currentWeaponScript.range = shotgunRange;
                currentWeaponScript.bulletsPerShot = shotgunBulletsPerShot;
                currentWeaponScript.spread = shotgunSpread;
            }

            Debug.Log("Cambiado a Escopeta");
        }

        // Sincronizar con otros jugadores
        photonView.RPC("SyncWeaponSwitch", RpcTarget.OthersBuffered, weaponIndex);
    }

    [PunRPC]
    void SyncWeaponSwitch(int weaponIndex)
    {
        // Solo visual para otros jugadores
        if (pistol != null) pistol.SetActive(weaponIndex == 0);
        if (shotgun != null) shotgun.SetActive(weaponIndex == 1);
    }
}