using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
{
    public GameObject playerPrefab;
    public Transform spawnPoint;

    public bool showLogs = true;

    void Start()
    {
        NicknameConfig();

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.AddCallbackTarget(this);

        ConectPhoton();
    }

    void NicknameConfig()
    {
        if (GameManager.Instance != null)
        {
            string nickname = GameManager.Instance.GetNickname();
            PhotonNetwork.NickName = nickname;

            if (showLogs)
                Debug.Log("Nickname" + PhotonNetwork.NickName);
        }
        else
        {
            PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
            Debug.LogWarning("GameManager no" + PhotonNetwork.NickName);
        }
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void ConectPhoton()
    {
        if (showLogs) Debug.Log("Connecting...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnConnectedToMaster()
    {
        if (showLogs) Debug.Log("Conectado al servidor maestro de Photon");
        PhotonNetwork.JoinOrCreateRoom("SalaJuego", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public void OnJoinedRoom()
    {
        if (showLogs) Debug.Log("Unido a la sala: " + PhotonNetwork.CurrentRoom.Name);
        if (showLogs) Debug.Log("Jugadores en la sala: " + PhotonNetwork.CurrentRoom.PlayerCount);

        SpawnPlayer();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (showLogs) Debug.Log("Nuevo jugador entró: " + newPlayer.NickName);
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (showLogs) Debug.Log("Jugador salió: " + otherPlayer.NickName);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (showLogs) Debug.LogWarning("Desconectado de Photon: " + cause);
    }

    public void OnConnected() { }
    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }
    public void OnCreatedRoom() { }
    public void OnCreateRoomFailed(short returnCode, string message)
    {
        if (showLogs) Debug.LogError("Error creando sala: " + message);
    }
    public void OnJoinRoomFailed(short returnCode, string message)
    {
        if (showLogs) Debug.LogError("Error uniéndose a sala: " + message);
    }
    public void OnJoinRandomFailed(short returnCode, string message) { }
    public void OnLeftRoom() { }
    public void OnFriendListUpdate(List<FriendInfo> friendList) { }


    void SpawnPlayer()
    {
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        spawnPosition += new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        if (showLogs) Debug.Log("Player: " + player.name + PhotonNetwork.NickName);
    }
}