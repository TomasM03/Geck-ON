using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponInventory : MonoBehaviourPun
{
    [Header("Starting Weapon")]
    public WeaponData startingWeapon;

    [Header("UI")]
    public TMP_Text weaponNameText;

    [Header("Settings")]
    public int maxWeapons = 3;
    public bool showLogs = true;

    private List<WeaponData> weapons = new List<WeaponData>();
    private int currentWeaponIndex = 0;

    private WeaponController weaponController;

    void Start()
    {
        if (!photonView.IsMine)
            return;

        weaponController = GetComponent<WeaponController>();

        // Agregar arma inicial
        if (startingWeapon != null)
        {
            AddWeapon(startingWeapon);
        }
        else
        {
            Debug.LogError("No hay arma inicial asignada!");
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        HandleInput();
    }

    void HandleInput()
    {
        // Cambio con teclas numéricas
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchWeapon(2);
        }

        // Cambio con scroll del mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SwitchWeapon(currentWeaponIndex - 1);
        }
        else if (scroll < 0f)
        {
            SwitchWeapon(currentWeaponIndex + 1);
        }
    }

    public void AddWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null)
        {
            Debug.LogWarning("Intentando agregar arma nula");
            return;
        }

        // Verificar si ya tiene el arma
        if (HasWeapon(newWeapon))
        {
            if (showLogs) Debug.Log("Ya tienes esta arma: " + newWeapon.weaponName);
            return;
        }

        // Verificar límite de armas
        if (weapons.Count >= maxWeapons)
        {
            if (showLogs) Debug.Log("Inventario lleno. Reemplazando arma actual.");
            weapons[currentWeaponIndex] = newWeapon;
        }
        else
        {
            weapons.Add(newWeapon);
            currentWeaponIndex = weapons.Count - 1; // Cambiar a la nueva arma
        }

        if (showLogs) Debug.Log("Arma agregada: " + newWeapon.weaponName);

        UpdateWeapon();
    }

    public bool HasWeapon(WeaponData weapon)
    {
        foreach (WeaponData w in weapons)
        {
            if (w == weapon)
                return true;
        }
        return false;
    }

    void SwitchWeapon(int index)
    {
        if (weapons.Count == 0)
            return;

        // Wrap around (ciclo infinito)
        if (index < 0)
            index = weapons.Count - 1;
        else if (index >= weapons.Count)
            index = 0;

        currentWeaponIndex = index;
        UpdateWeapon();
    }

    void UpdateWeapon()
    {
        if (weapons.Count == 0 || currentWeaponIndex >= weapons.Count)
            return;

        WeaponData currentWeapon = weapons[currentWeaponIndex];

        // Notificar al WeaponController
        if (weaponController != null)
        {
            weaponController.SetWeapon(currentWeapon);
        }

        // Actualizar visuals del arma
        WeaponVisuals weaponVisuals = GetComponent<WeaponVisuals>();
        if (weaponVisuals != null)
        {
            weaponVisuals.ShowWeapon(currentWeapon.weaponName);
        }

        // Actualizar UI
        UpdateUI();

        // Sincronizar con otros jugadores (opcional)
        if (photonView.IsMine)
        {
            photonView.RPC("SyncWeapon", RpcTarget.OthersBuffered, currentWeaponIndex);
        }

        if (showLogs) Debug.Log("Arma equipada: " + currentWeapon.weaponName);
    }

    [PunRPC]
    void SyncWeapon(int weaponIndex)
    {
        currentWeaponIndex = weaponIndex;
        // Aquí podrías actualizar visuals del arma para otros jugadores
    }

    void UpdateUI()
    {
        if (weaponNameText != null && weapons.Count > 0)
        {
            WeaponData currentWeapon = weapons[currentWeaponIndex];
            weaponNameText.text = currentWeapon.weaponName + " (" + (currentWeaponIndex + 1) + "/" + weapons.Count + ")";
        }
    }

    public WeaponData GetCurrentWeapon()
    {
        if (weapons.Count == 0)
            return null;

        return weapons[currentWeaponIndex];
    }

    public int GetWeaponCount()
    {
        return weapons.Count;
    }
}