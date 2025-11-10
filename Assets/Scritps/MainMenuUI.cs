using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

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

        if (PhotonNetwork.IsConnected)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 10;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            PhotonNetwork.JoinOrCreateRoom("GameRoom", roomOptions, TypedLobby.Default);
        }

        nicknamePanel.SetActive(false);

        if (lobbyUI != null)
        {
            lobbyUI.OpenLobby();
        }
    }
}
