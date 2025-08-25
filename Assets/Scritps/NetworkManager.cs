using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
{
    [Header("Configuration")]
    public GameObject playerPrefab;
    public Transform spawnPoint;

    [Header("Debug")]
    public bool showLogs = true;

    void Start()
    {
        // Configure Photon
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.AddCallbackTarget(this);

        // Only connect automatically if we're NOT in the main menu
        // The main menu will handle the connection
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
        {
            ConnectToPhoton();
        }
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void ConnectToPhoton()
    {
        if (showLogs) Debug.Log("Connecting to Photon...");

        // Configure region (optional, uses closest automatically)
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Photon Callbacks

    public void OnConnectedToMaster()
    {
        if (showLogs) Debug.Log("Connected to Photon Master Server");

        // Join or create room automatically
        PhotonNetwork.JoinOrCreateRoom("GameRoom", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public void OnJoinedRoom()
    {
        if (showLogs)
        {
            Debug.Log("=== PLAYER CONNECTED ===");
            Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
            Debug.Log("MY ID: " + PhotonNetwork.LocalPlayer.ActorNumber);
            Debug.Log("MY NAME: " + PhotonNetwork.LocalPlayer.NickName);
            Debug.Log("Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount);
            Debug.Log("========================");
        }

        // Spawn player when we join the room
        SpawnPlayer();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (showLogs)
        {
            Debug.Log("=== NEW PLAYER ===");
            Debug.Log("Name: " + newPlayer.NickName);
            Debug.Log("ID: " + newPlayer.ActorNumber);
            Debug.Log("Total players: " + PhotonNetwork.CurrentRoom.PlayerCount);
            Debug.Log("==================");
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (showLogs) Debug.Log("Player left: " + otherPlayer.NickName);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (showLogs) Debug.LogWarning("Disconnected from Photon: " + cause);
    }

    // Required methods by interfaces (but we don't use them)
    public void OnConnected() { }
    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }
    public void OnCreatedRoom() { }
    public void OnCreateRoomFailed(short returnCode, string message) { }
    public void OnJoinRoomFailed(short returnCode, string message) { }
    public void OnJoinRandomFailed(short returnCode, string message) { }
    public void OnLeftRoom() { }
    public void OnFriendListUpdate(System.Collections.Generic.List<FriendInfo> friendList) { }

    #endregion

    void SpawnPlayer()
    {
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        // Add some variation in position to avoid spawning in the same place
        spawnPosition += new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));

        // Instantiate player through the network
        GameObject myPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        if (showLogs) Debug.Log("Player spawned: " + myPlayer.name);
    }

    void OnGUI()
    {
        // Simple UI to show connection status
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));

        GUILayout.Label("Photon Status: " + PhotonNetwork.NetworkClientState);

        if (PhotonNetwork.InRoom)
        {
            GUILayout.Label("Room: " + PhotonNetwork.CurrentRoom.Name);
            GUILayout.Label("Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);
        }

        GUILayout.EndArea();
    }
}