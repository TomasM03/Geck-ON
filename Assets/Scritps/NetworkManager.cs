using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviourPunCallbacks
{
<<<<<<< HEAD
    public static NetworkManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
=======
    [Header("Configuración")]
    public GameObject jugadorPrefab;
    public Transform puntoSpawn;

    [Header("Debug")]
    public bool mostrarLogs = true;

    void Start()
    {
        // Configurar Photon
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.AddCallbackTarget(this);

        // Conectar a Photon
        ConectarAPhoton();
>>>>>>> parent of 2d2f8a7 (Nickname/TMP)
    }

    public void LeaveRoom()
    {
<<<<<<< HEAD
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("Leaving room...");
=======
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void ConectarAPhoton()
    {
        if (mostrarLogs) Debug.Log("Conectando a Photon...");

        // Configurar la región (opcional, usa la más cercana automáticamente)
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Callbacks de Photon

    public void OnConnectedToMaster()
    {
        if (mostrarLogs) Debug.Log("Conectado al servidor maestro de Photon");

        // Unirse o crear una sala automáticamente
        PhotonNetwork.JoinOrCreateRoom("SalaJuego", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public void OnJoinedRoom()
    {
        if (mostrarLogs) Debug.Log("Unido a la sala: " + PhotonNetwork.CurrentRoom.Name);
        if (mostrarLogs) Debug.Log("Jugadores en la sala: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // Spawnear el jugador cuando nos unimos a la sala
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

    // Métodos requeridos por las interfaces (pero no los usamos)
    public void OnConnected() { }
    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }
    public void OnCreatedRoom() { }
    public void OnCreateRoomFailed(short returnCode, string message) { }
    public void OnJoinRoomFailed(short returnCode, string message) { }
    public void OnJoinRandomFailed(short returnCode, string message) { }
    public void OnLeftRoom() { }
    public void OnFriendListUpdate(List<FriendInfo> friendList) { }

    #endregion

    void SpawnearJugador()
    {
        Vector3 posicionSpawn = puntoSpawn != null ? puntoSpawn.position : Vector3.zero;

        // Agregar algo de variación en la posición para evitar que spawnen en el mismo lugar
        posicionSpawn += new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));

        // Instanciar el jugador a través de la red
        GameObject miJugador = PhotonNetwork.Instantiate(jugadorPrefab.name, posicionSpawn, Quaternion.identity);

        if (mostrarLogs) Debug.Log("Jugador spawneado: " + miJugador.name);
    }

    void OnGUI()
    {
        // UI simple para mostrar el estado de la conexión
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));

        GUILayout.Label("Estado de Photon: " + PhotonNetwork.NetworkClientState);

        if (PhotonNetwork.InRoom)
        {
            GUILayout.Label("Sala: " + PhotonNetwork.CurrentRoom.Name);
            GUILayout.Label("Jugadores: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);
>>>>>>> parent of 2d2f8a7 (Nickname/TMP)
        }
    }

    public void Disconnect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            Debug.Log("Disconnecting from Photon...");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player joined: {newPlayer.NickName}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player left: {otherPlayer.NickName}");
    }
}
