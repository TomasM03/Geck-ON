using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwitcher : MonoBehaviourPun
{
    public Transform weaponHolder;

    public GameObject[] weapons;
    private int currentWeapon = 0;

    public Image weaponUI1;
    public Image weaponUI2;

    public Sprite[] weaponSprites;

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
        UpdateUIWeapon();
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(0);
            UpdateUIWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(1);
            UpdateUIWeapon();
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

    private void UpdateUIWeapon()
    {
        if(currentWeapon == 0)
        {
            weaponUI1.sprite = weaponSprites[0];
            weaponUI2.sprite = weaponSprites[1];
        }
        if (currentWeapon == 1)
        {
            weaponUI1.sprite = weaponSprites[2];
            weaponUI2.sprite = weaponSprites[3];
        }
    }
}
