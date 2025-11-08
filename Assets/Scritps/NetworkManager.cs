using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Player")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] teamASpawns;
    public Transform[] teamBSpawns;

    private bool hasSpawned = false;

    void Start()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log("NetworkManager Start en escena: " + sceneName);

        // SOLO conectar si estamos en MainMenu Y no estamos conectados
        if (sceneName == "MainMenu")
        {
            if (!PhotonNetwork.IsConnected)
            {
                ConfigurePhoton();
                PhotonNetwork.ConnectUsingSettings();
                Debug.Log("Conectando a Photon...");
            }
            else
            {
                Debug.Log("Ya conectado a Photon");
            }
        }
        // Si estamos en GameScene, verificar y spawnear
        else
        {
            Debug.Log("Estamos en GameScene. IsConnected: " + PhotonNetwork.IsConnected + " | InRoom: " + PhotonNetwork.InRoom);

            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && !hasSpawned)
            {
                Debug.Log("Intentando spawnear jugador...");
                SpawnPlayer();
            }
            else
            {
                Debug.LogWarning("No se puede spawnear aún. Esperando conexión...");
            }
        }
    }

    void ConfigurePhoton()
    {
        // Configurar nickname
        if (GameManager.Instance != null)
        {
            PhotonNetwork.NickName = GameManager.Instance.GetNickname();
        }
        else
        {
            PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Photon configurado. Nickname: " + PhotonNetwork.NickName);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado a Photon Master Server");

        // Solo unirse a sala si estamos en MainMenu
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 10;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;

            PhotonNetwork.JoinOrCreateRoom("GameRoom", roomOptions, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Unido a sala: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Jugadores en sala: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // Si estamos en escena de juego, spawnear
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName != "MainMenu" && !hasSpawned)
        {
            Debug.Log("OnJoinedRoom: Intentando spawnear...");
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        if (hasSpawned)
        {
            Debug.LogWarning("Ya spawneaste!");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("¡No hay Player Prefab asignado en NetworkManager!");
            return;
        }

        // Verificar que el jugador tenga equipo asignado
        string myTeam = "";
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
        {
            myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            Debug.Log("Mi equipo: " + myTeam);
        }
        else
        {
            Debug.LogError("¡No tienes equipo asignado! Debes elegir equipo en el lobby.");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition(myTeam);

        Debug.Log("Spawneando prefab: " + playerPrefab.name + " en posición: " + spawnPos);

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);

        hasSpawned = true;
        Debug.Log("¡Player spawneado exitosamente! GameObject: " + player.name);
    }

    public Vector3 GetSpawnPosition(string team)
    {
        Transform[] spawns = null;

        if (team == "A" && teamASpawns != null && teamASpawns.Length > 0)
        {
            spawns = teamASpawns;
            Debug.Log("Usando spawn del Team A (" + spawns.Length + " puntos disponibles)");
        }
        else if (team == "B" && teamBSpawns != null && teamBSpawns.Length > 0)
        {
            spawns = teamBSpawns;
            Debug.Log("Usando spawn del Team B (" + spawns.Length + " puntos disponibles)");
        }

        if (spawns != null && spawns.Length > 0)
        {
            Transform selectedSpawn = spawns[Random.Range(0, spawns.Length)];
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Vector3 finalPos = selectedSpawn.position + offset;
            Debug.Log("Spawn point seleccionado: " + selectedSpawn.name + " -> " + finalPos);
            return finalPos;
        }

        // Fallback si no hay spawns configurados
        Debug.LogWarning("¡No hay spawns configurados para el equipo " + team + "! Usando posición por defecto.");
        return new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Desconectado de Photon: " + cause);
        hasSpawned = false;
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Saliste de la sala");
        hasSpawned = false;
    }
}