using UnityEngine;
using Photon.Pun;

public class WeaponSwitcher : MonoBehaviourPun
{
    [Header("Weapons")]
    public GameObject pistol;
    public GameObject shotgun;

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
    public int shotgunBulletsPerShot = 8; 
    public float shotgunSpread = 5f; 

    private int currentWeaponIndex = 0;
    private Weapon currentWeaponScript;

    void Start()
    {
        if (!photonView.IsMine) return;

        SwitchToWeapon(0);
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToWeapon(1);
        }
    }

    void SwitchToWeapon(int weaponIndex)
    {
        if (weaponIndex == currentWeaponIndex) return;

        currentWeaponIndex = weaponIndex;

        if (pistol != null) pistol.SetActive(false);
        if (shotgun != null) shotgun.SetActive(false);

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

            Debug.Log("Pistola");
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

            Debug.Log("Escopeta");
        }

        photonView.RPC("SyncWeaponSwitch", RpcTarget.OthersBuffered, weaponIndex);
    }

    [PunRPC]
    void SyncWeaponSwitch(int weaponIndex)
    {
        if (pistol != null) pistol.SetActive(weaponIndex == 0);
        if (shotgun != null) shotgun.SetActive(weaponIndex == 1);
    }
}