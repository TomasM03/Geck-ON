using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
{
    [Header("Configuraci�n")]
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
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void ConectarAPhoton()
    {
        if (mostrarLogs) Debug.Log("Conectando a Photon...");

        // Configurar la regi�n (opcional, usa la m�s cercana autom�ticamente)
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Callbacks de Photon

    public void OnConnectedToMaster()
    {
        if (mostrarLogs) Debug.Log("Conectado al servidor maestro de Photon");

        // Unirse o crear una sala autom�ticamente
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
        if (mostrarLogs) Debug.Log("Nuevo jugador entr�: " + newPlayer.NickName);
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (mostrarLogs) Debug.Log("Jugador sali�: " + otherPlayer.NickName);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (mostrarLogs) Debug.LogWarning("Desconectado de Photon: " + cause);
    }

    // M�todos requeridos por las interfaces (pero no los usamos)
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

        // Agregar algo de variaci�n en la posici�n para evitar que spawnen en el mismo lugar
        posicionSpawn += new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));

        // Instanciar el jugador a trav�s de la red
        GameObject miJugador = PhotonNetwork.Instantiate(jugadorPrefab.name, posicionSpawn, Quaternion.identity);

        if (mostrarLogs) Debug.Log("Jugador spawneado: " + miJugador.name);
    }

    void OnGUI()
    {
        // UI simple para mostrar el estado de la conexi�n
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));

        GUILayout.Label("Estado de Photon: " + PhotonNetwork.NetworkClientState);

        if (PhotonNetwork.InRoom)
        {
            GUILayout.Label("Sala: " + PhotonNetwork.CurrentRoom.Name);
            GUILayout.Label("Jugadores: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);
        }

        GUILayout.EndArea();
    }
}