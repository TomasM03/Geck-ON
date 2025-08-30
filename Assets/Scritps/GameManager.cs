using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string nickname = "";
    public int armaSeleccionada = 0;
    public int skinSeleccionado = 0;

    public bool mostrarLogs = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CargarDatosGuardados();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetNickname(string nuevoNickname)
    {
        nickname = nuevoNickname;
        GuardarDatos();
        if (mostrarLogs) Debug.Log("Nickname guardado: " + nickname);
    }

    public void SetArma(int armaIndex)
    {
        armaSeleccionada = armaIndex;
        GuardarDatos();
        if (mostrarLogs) Debug.Log("Arma seleccionada: " + armaIndex);
    }

    public void SetSkin(int skinIndex)
    {
        skinSeleccionado = skinIndex;
        GuardarDatos();
        if (mostrarLogs) Debug.Log("Skin seleccionado: " + skinIndex);
    }

    public string GetNickname()
    {
        if (string.IsNullOrEmpty(nickname))
        {
            nickname = "Jugador_" + Random.Range(1000, 9999);
            GuardarDatos();
        }
        return nickname;
    }

    public int GetArmaSeleccionada()
    {
        return armaSeleccionada;
    }

    public int GetSkinSeleccionado()
    {
        return skinSeleccionado;
    }

    void GuardarDatos()
    {
        PlayerPrefs.SetString("PlayerNickname", nickname);
        PlayerPrefs.SetInt("PlayerArma", armaSeleccionada);
        PlayerPrefs.SetInt("PlayerSkin", skinSeleccionado);
        PlayerPrefs.Save();
    }

    void CargarDatosGuardados()
    {
        nickname = PlayerPrefs.GetString("PlayerNickname", "");
        armaSeleccionada = PlayerPrefs.GetInt("PlayerArma", 0);
        skinSeleccionado = PlayerPrefs.GetInt("PlayerSkin", 0);

        if (mostrarLogs)
        {
            Debug.Log("Datos cargados - Nickname: " + nickname +
                     ", Arma: " + armaSeleccionada +
                     ", Skin: " + skinSeleccionado);
        }
    }

    public void BorrarDatos()
    {
        PlayerPrefs.DeleteKey("PlayerNickname");
        PlayerPrefs.DeleteKey("PlayerArma");
        PlayerPrefs.DeleteKey("PlayerSkin");
        PlayerPrefs.Save();

        nickname = "";
        armaSeleccionada = 0;
        skinSeleccionado = 0;

        if (mostrarLogs) Debug.Log("Datos del jugador borrados");
    }
}