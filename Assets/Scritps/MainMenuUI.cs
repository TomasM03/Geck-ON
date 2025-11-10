using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI - Nickname Screen")]
    public GameObject nicknamePanel;
    public TMP_InputField nicknameInput;
    public Button confirmNicknameButton;
    public Button quiButton;

    [Header("UI - Lobby")]
    public LobbyUI lobbyUI;

    void Start()
    {
        nicknamePanel.SetActive(true);

        confirmNicknameButton.onClick.AddListener(OnConfirmNickname);
        quiButton.onClick.AddListener(QuitGame);

        if (GameManager.Instance != null)
        {
            string savedNick = GameManager.Instance.GetNickname();
            if (!string.IsNullOrEmpty(savedNick))
            {
                nicknameInput.text = savedNick;
            }
        }
    }

    void QuitGame()
    {
        Application.Quit();
    }

    void OnConfirmNickname()
    {
        string nick = nicknameInput.text.Trim();

        if (string.IsNullOrEmpty(nick))
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetNickname(nick);
        }

        nicknamePanel.SetActive(false);

        if (lobbyUI != null)
        {
            lobbyUI.OpenLobby();
        }
    }
}
