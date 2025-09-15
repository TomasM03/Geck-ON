using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public TMP_InputField nicknameInput;
    public Button playButton;
    public Button quitButton;

    public string gameScene = "GameScene";

    public bool Logs = true;

    void Start()
    {
        SetUI();
        Upload();
    }

    //Void Sets
    void SetUI()
    {
        if (playButton != null)
            playButton.onClick.AddListener(Play);

        if (quitButton != null)
            quitButton.onClick.AddListener(SalirJuego);

        if (nicknameInput != null)
        {
            nicknameInput.onEndEdit.AddListener(NicknameRefresh);
            nicknameInput.characterLimit = 20;
        }
    }


    void Upload()
    {
        if (GameManager.Instance != null && nicknameInput != null)
        {
            string nicknameGuardado = GameManager.Instance.GetNickname();
            if (!string.IsNullOrEmpty(nicknameGuardado) &&
                !nicknameGuardado.StartsWith("Player_"))
            {
                nicknameInput.text = nicknameGuardado;
            }
        }
    }

    //Nick Refresh
    public void NicknameRefresh(string nuevoNickname)
    {
        nuevoNickname = nuevoNickname.Trim();

        if (string.IsNullOrEmpty(nuevoNickname))
        {
            if (Logs) Debug.Log("Nickname vacío");
            return;
        }

        nuevoNickname = NickNameFilter(nuevoNickname);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetNickname(nuevoNickname);
        }

        if (Logs) Debug.Log("Nickname actualizado: " + nuevoNickname);
    }

    string NickNameFilter(string nickname)
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

    //Play Button
    public void Play()
    {
        if (nicknameInput != null)
        {
            NicknameRefresh(nicknameInput.text);
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no");
            return;
        }

        if (Logs)
        {
            Debug.Log("Nickname: " + GameManager.Instance.GetNickname());
        }

        SceneManager.LoadScene(gameScene);
    }

    public void SalirJuego()
    {
        Application.Quit();
    }
}