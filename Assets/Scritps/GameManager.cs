using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string nickname = "";
    public int weaponSelect = 0;

    public bool mostrarLogs = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInfo();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void SetNickname(string nuevoNickname)
    {
        nickname = nuevoNickname;
        SaveInfos();
        if (mostrarLogs) Debug.Log("Nickname guardado: " + nickname);
    }

    public void SetWeapon(int armaIndex)
    {
        weaponSelect = armaIndex;
        SaveInfos();
        if (mostrarLogs) Debug.Log("Arma seleccionada: " + armaIndex);
    }

    public string GetNickname()
    {
        if (string.IsNullOrEmpty(nickname))
        {
            nickname = "Jugador_" + Random.Range(1000, 9999);
            SaveInfos();
        }
        return nickname;
    }

    public int GetWeapon()
    {
        return weaponSelect;
    }

    void SaveInfos()
    {
        PlayerPrefs.SetString("PlayerNickname", nickname);
        PlayerPrefs.SetInt("PlayerArma", weaponSelect);
        PlayerPrefs.Save();
    }

    void LoadInfo()
    {
        nickname = PlayerPrefs.GetString("PlayerNickname", "");
        weaponSelect = PlayerPrefs.GetInt("PlayerArma", 0);

        if (mostrarLogs)
        {
            Debug.Log("Datos cargados - Nickname: " + nickname +
                     ", Arma: " + weaponSelect);
        }
    }

    public void DeletInfo()
    {
        PlayerPrefs.DeleteKey("PlayerNickname");
        PlayerPrefs.DeleteKey("PlayerArma");
        PlayerPrefs.DeleteKey("PlayerSkin");
        PlayerPrefs.Save();

        nickname = "";
        weaponSelect = 0;

        if (mostrarLogs) Debug.Log("Datos del jugador borrados");
    }
}