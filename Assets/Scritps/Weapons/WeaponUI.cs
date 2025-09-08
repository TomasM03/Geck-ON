using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text weaponNameText;
    public Text ammoText;

    private Weapon currentWeapon;
    private WeaponSwitcher weaponSwitcher;

    void Start()
    {
        PhotonView[] players = FindObjectsOfType<PhotonView>();

        foreach (var player in players)
        {
            if (player.IsMine)
            {
                weaponSwitcher = player.GetComponent<WeaponSwitcher>();
                break;
            }
        }
    }

    void Update()
    {
        if (weaponSwitcher == null)
            return;

        foreach (GameObject weapon in weaponSwitcher.weapons)
        {
            if (weapon != null && weapon.activeInHierarchy)
            {
                Weapon weaponScript = weapon.GetComponent<Weapon>();
                if (weaponScript != currentWeapon)
                {
                    currentWeapon = weaponScript;
                    UpdateUI();
                }
                break;
            }
        }
    }

    void UpdateUI()
    {
        if (currentWeapon != null && weaponNameText != null)
        {
            weaponNameText.text = currentWeapon.weaponName;
        }
    }
}
