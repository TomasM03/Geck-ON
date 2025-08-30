using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TMP_InputField nicknameInput;
    public Button playButton;
    public Button quitButton;

    public string escenaJuego = "GameScene";

    public bool mostrarLogs = true;

    void Start()
    {
        ConfigurarUI();
        CargarDatosGuardados();
    }

    void ConfigurarUI()
    {
        if (playButton != null)
            playButton.onClick.AddListener(IniciarJuego);

        if (quitButton != null)
            quitButton.onClick.AddListener(SalirJuego);

        if (nicknameInput != null)
        {
            nicknameInput.onEndEdit.AddListener(ActualizarNickname);
            nicknameInput.characterLimit = 20;
        }
    }

    void CargarDatosGuardados()
    {
        if (GameManager.Instance != null && nicknameInput != null)
        {
            string nicknameGuardado = GameManager.Instance.GetNickname();
            if (!string.IsNullOrEmpty(nicknameGuardado) &&
                !nicknameGuardado.StartsWith("Jugador_"))
            {
                nicknameInput.text = nicknameGuardado;
            }
        }
    }

    public void ActualizarNickname(string nuevoNickname)
    {
        nuevoNickname = nuevoNickname.Trim();

        if (string.IsNullOrEmpty(nuevoNickname))
        {
            if (mostrarLogs) Debug.Log("Nickname vacío, se generará uno automático");
            return;
        }

        nuevoNickname = FiltrarNickname(nuevoNickname);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetNickname(nuevoNickname);
        }

        if (mostrarLogs) Debug.Log("Nickname actualizado: " + nuevoNickname);
    }

    string FiltrarNickname(string nickname)
    {
        string caracteresPermitidos = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
        string nicknameFiltrado = "";

        foreach (char c in nickname)
        {
            if (caracteresPermitidos.Contains(c.ToString()))
            {
                nicknameFiltrado += c;
            }
        }

        return nicknameFiltrado;
    }

    public void IniciarJuego()
    {
        if (nicknameInput != null)
        {
            ActualizarNickname(nicknameInput.text);
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no encontrado!");
            return;
        }

        if (mostrarLogs)
        {
            Debug.Log("Iniciando juego con nickname: " + GameManager.Instance.GetNickname());
        }

        SceneManager.LoadScene(escenaJuego);
    }

    public void SalirJuego()
    {
        if (mostrarLogs) Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void BorrarDatosGuardados()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BorrarDatos();

            if (nicknameInput != null)
                nicknameInput.text = "";

            if (mostrarLogs) Debug.Log("Datos borrados");
        }
    }

    void OnGUI()
    {
        // Debug UI (opcional)
        if (mostrarLogs && GameManager.Instance != null)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUILayout.Label("DEBUG - GameManager");
            GUILayout.Label("Nickname: " + GameManager.Instance.GetNickname());
            GUILayout.Label("Arma: " + GameManager.Instance.GetArmaSeleccionada());
            GUILayout.Label("Skin: " + GameManager.Instance.GetSkinSeleccionado());
            GUILayout.EndArea();
        }
    }
}