using Photon.Pun;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviourPun
{
    public Transform weaponHolder;

    public GameObject[] weapons;
    private int currentWeapon = 0;

    void Start()
    {
        if (!photonView.IsMine)
            return;

        if (weaponHolder == null)
        {
            GameObject holder = new GameObject("WeaponHolder");
            holder.transform.SetParent(transform);
            holder.transform.localPosition = Vector3.zero;
            weaponHolder = holder.transform;
        }

        SwitchWeapon(0);

        if (GameManager.Instance != null)
        {
            int savedWeapon = GameManager.Instance.GetWeapon();
            if (savedWeapon < weapons.Length)
            {
                SwitchWeapon(savedWeapon);
            }
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(1);
        }
    }

    void SwitchWeapon(int weaponIndex)
    {
        if (weapons == null || weaponIndex < 0 || weaponIndex >= weapons.Length)
            return;

        if (weapons[currentWeapon] != null)
            weapons[currentWeapon].SetActive(false);

        currentWeapon = weaponIndex;
        if (weapons[currentWeapon] != null)
        {
            weapons[currentWeapon].SetActive(true);

            Weapon weaponScript = weapons[currentWeapon].GetComponent<Weapon>();
            Debug.Log("Arma cambiada a: " + weaponScript.weaponName);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetWeapon(weaponIndex);
            }
        }
    }
}
