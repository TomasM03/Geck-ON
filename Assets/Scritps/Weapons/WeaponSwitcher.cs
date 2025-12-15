using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class WeaponSwitcher : MonoBehaviourPun
{
    public GameObject pistol;
    public GameObject shotgun;

    public float pistolDamage = 25f;
    public float pistolFireRate = 0.15f;
    public float pistolRange = 100f;
    public int pistolBulletsPerShot = 1;
    public float pistolSpread = 0.5f;

    public float shotgunDamage = 15f;
    public float shotgunFireRate = 0.8f;
    public float shotgunRange = 30f;
    public int shotgunBulletsPerShot = 8;
    public float shotgunSpread = 5f;

    private int currentWeaponIndex = 0;
    private Weapon currentWeaponScript;

    public Sprite pistol0;
    public Sprite pistol1;
    public Sprite shotgun0;
    public Sprite shotgun1;

    public Image weapon1;
    public Image weapon2;
    public Image weapon3;


    void Start()
    {
        if (!photonView.IsMine) return;
        SwitchToWeapon(0);
        UpdateUIWeapon();
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
        }

        UpdateUIWeapon();
        photonView.RPC("SyncWeaponSwitch", RpcTarget.OthersBuffered, weaponIndex);
    }

    [PunRPC]
    void SyncWeaponSwitch(int weaponIndex)
    {
        if (pistol != null) pistol.SetActive(weaponIndex == 0);
        if (shotgun != null) shotgun.SetActive(weaponIndex == 1);
    }

    private void UpdateUIWeapon()
    {
        if (currentWeaponIndex == 0)
        {
            weapon1.sprite = pistol1;
            weapon2.sprite = shotgun0;
        }
        else if (currentWeaponIndex == 1)
        {
            weapon1.sprite = pistol0;
            weapon2.sprite = shotgun1;
        }
    }
}