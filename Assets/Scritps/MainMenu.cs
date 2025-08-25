using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour, IConnectionCallbacks
{
    [Header("UI Elements")]
    public TMP_InputField nameInputField;
    public Button connectButton;
    public TMP_Text statusText;
    public TMP_Text instructionsText;

    [Header("Configuration")]
    public string gameSceneName = "Prueba1";

    private bool isConnecting = false;

    void Start()
    {
        ConfigureUI();
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void ConfigureUI()
    {
        if (instructionsText != null)
            instructionsText.text = "Enter your name and connect to the server";

        if (connectButton != null)
        {
            connectButton.onClick.AddListener(ConnectToServer);
            connectButton.interactable = true;
        }

        if (nameInputField != null)
            nameInputField.text = "Player" + Random.Range(1, 1000);

        UpdateStatusUI("Ready to connect");
    }

    public void ConnectToServer()
    {
        if (isConnecting) return;

        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            UpdateStatusUI("You must enter a name!");
            return;
        }

        if (playerName.Length < 2)
        {
            UpdateStatusUI("Name must have at least 2 characters");
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = playerName;

        isConnecting = true;
        connectButton.interactable = false;
        UpdateStatusUI("Connecting to Photon...");

        Debug.Log("Trying to connect with name: " + playerName);
        PhotonNetwork.ConnectUsingSettings();
    }

    void UpdateStatusUI(string message)
    {
        if (statusText != null)
            statusText.text = "Status: " + message;
        Debug.Log("Status: " + message);
    }

    #region Photon Callbacks

    public void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        UpdateStatusUI("Connected! Joining room...");

        PhotonNetwork.JoinOrCreateRoom("GameRoom", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public void OnJoinedRoom()
    {
        Debug.Log("Joined room! Loading game...");
        UpdateStatusUI("Connected successfully! Loading game...");

        SceneManager.LoadScene(gameSceneName);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected: " + cause);
        UpdateStatusUI("Connection error: " + cause);

        isConnecting = false;
        if (connectButton != null)
            connectButton.interactable = true;
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
        UpdateStatusUI("Failed to join room");

        isConnecting = false;
        if (connectButton != null)
            connectButton.interactable = true;
    }

    public void OnConnected() { }
    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }

    #endregion

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnNameChanged()
    {
        if (nameInputField != null && connectButton != null)
        {
            bool validName = !string.IsNullOrEmpty(nameInputField.text.Trim()) &&
                            nameInputField.text.Trim().Length >= 2;
            connectButton.interactable = validName && !isConnecting;
        }
    }
}
