using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
{
    [Header("Configuración")]
    public GameObject jugadorPrefab;
    public Transform puntoSpawn;

    [Header("Debug")]
    public bool mostrarLogs = true;

    void Start()
    {
        ConfigurarNickname();

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.AddCallbackTarget(this);

        ConectarAPhoton();
    }

    void ConfigurarNickname()
    {
        if (GameManager.Instance != null)
        {
            string nickname = GameManager.Instance.GetNickname();
            PhotonNetwork.NickName = nickname;

            if (mostrarLogs)
                Debug.Log("Nickname configurado para Photon: " + PhotonNetwork.NickName);
        }
        else
        {
            PhotonNetwork.NickName = "Jugador_" + Random.Range(1000, 9999);
            Debug.LogWarning("GameManager no encontrado, usando nickname por defecto: " + PhotonNetwork.NickName);
        }
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void ConectarAPhoton()
    {
        if (mostrarLogs) Debug.Log("Conectando a Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Callbacks de Photon

    public void OnConnectedToMaster()
    {
        if (mostrarLogs) Debug.Log("Conectado al servidor maestro de Photon");
        PhotonNetwork.JoinOrCreateRoom("SalaJuego", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public void OnJoinedRoom()
    {
        if (mostrarLogs) Debug.Log("Unido a la sala: " + PhotonNetwork.CurrentRoom.Name);
        if (mostrarLogs) Debug.Log("Jugadores en la sala: " + PhotonNetwork.CurrentRoom.PlayerCount);
        
        SpawnearJugador();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (mostrarLogs) Debug.Log("Nuevo jugador entró: " + newPlayer.NickName);
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (mostrarLogs) Debug.Log("Jugador salió: " + otherPlayer.NickName);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (mostrarLogs) Debug.LogWarning("Desconectado de Photon: " + cause);
    }

    public void OnConnected() { }
    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }
    public void OnCreatedRoom() { }
    public void OnCreateRoomFailed(short returnCode, string message)
    {
        if (mostrarLogs) Debug.LogError("Error creando sala: " + message);
    }
    public void OnJoinRoomFailed(short returnCode, string message)
    {
        if (mostrarLogs) Debug.LogError("Error uniéndose a sala: " + message);
    }
    public void OnJoinRandomFailed(short returnCode, string message) { }
    public void OnLeftRoom() { }
    public void OnFriendListUpdate(List<FriendInfo> friendList) { }

    #endregion

    void SpawnearJugador()
    {
        Vector3 posicionSpawn = puntoSpawn != null ? puntoSpawn.position : Vector3.zero;

        posicionSpawn += new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));

        GameObject miJugador = PhotonNetwork.Instantiate(jugadorPrefab.name, posicionSpawn, Quaternion.identity);

        if (mostrarLogs) Debug.Log("Jugador spawneado: " + miJugador.name + " con nickname: " + PhotonNetwork.NickName);
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Estado de Photon: " + PhotonNetwork.NetworkClientState);
        GUILayout.Label("Mi Nickname: " + PhotonNetwork.NickName);

        if (PhotonNetwork.InRoom)
        {
            GUILayout.Label("Sala: " + PhotonNetwork.CurrentRoom.Name);
            GUILayout.Label("Jugadores: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);

            GUILayout.Label("Jugadores conectados:");
            foreach (Player jugador in PhotonNetwork.PlayerList)
            {
                GUILayout.Label("- " + jugador.NickName);
            }
        }
        GUILayout.EndArea();
    }

    public void VolverAlMenu()
    {
        PhotonNetwork.LeaveRoom();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}