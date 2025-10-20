using UnityEngine;
using Photon.Pun;

public class WeaponVisuals : MonoBehaviourPun
{
    [System.Serializable]
    public class WeaponModel
    {
        public string weaponName;
        public GameObject model;
    }

    [Header("Weapon Models")]
    public WeaponModel[] weaponModels;

    [Header("Settings")]
    public bool showLogs = false;

    private GameObject currentModel;

    void Start()
    {
        // Desactivar todos los modelos al inicio
        HideAllModels();
    }

    public void ShowWeapon(string weaponName)
    {
        if (!photonView.IsMine)
            return;

        // Ocultar modelo actual
        if (currentModel != null)
        {
            currentModel.SetActive(false);
        }

        // Buscar y mostrar el nuevo modelo
        foreach (WeaponModel wm in weaponModels)
        {
            if (wm.weaponName == weaponName && wm.model != null)
            {
                wm.model.SetActive(true);
                currentModel = wm.model;

                if (showLogs)
                    Debug.Log("Mostrando modelo: " + weaponName);

                // Sincronizar visual con otros jugadores
                photonView.RPC("SyncWeaponVisual", RpcTarget.OthersBuffered, weaponName);
                return;
            }
        }

        Debug.LogWarning("No se encontró modelo para: " + weaponName);
    }

    [PunRPC]
    void SyncWeaponVisual(string weaponName)
    {
        // Ocultar modelo actual
        if (currentModel != null)
        {
            currentModel.SetActive(false);
        }

        // Mostrar el modelo sincronizado
        foreach (WeaponModel wm in weaponModels)
        {
            if (wm.weaponName == weaponName && wm.model != null)
            {
                wm.model.SetActive(true);
                currentModel = wm.model;
                return;
            }
        }
    }

    void HideAllModels()
    {
        foreach (WeaponModel wm in weaponModels)
        {
            if (wm.model != null)
            {
                wm.model.SetActive(false);
            }
        }
    }

    public void HideCurrentWeapon()
    {
        if (currentModel != null)
        {
            currentModel.SetActive(false);
        }
    }
}