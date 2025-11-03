using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameModeManager : MonoBehaviour, IMatchmakingCallbacks
{
    [Header("Game Mode Selection")]
    public GameModeType gameModeType = GameModeType.TeamDeathmatch;

    [Header("References")]
    public NetworkManager networkManager;

    [Header("Debug")]
    public bool showLogs = true;

    private GameModeBase activeGameMode;

    public static GameModeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }
    }

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void Start()
    {
        InitializeGameMode();
    }

    void Update()
    {
        if (activeGameMode != null)
        {
            activeGameMode.UpdateGameMode();
        }
    }

    void InitializeGameMode()
    {
        if (showLogs)
            Debug.Log("[GameModeManager] Inicializando modo: " + gameModeType);

        if (activeGameMode != null)
        {
            Destroy(activeGameMode);
        }

        switch (gameModeType)
        {
            case GameModeType.TeamDeathmatch:
                activeGameMode = gameObject.AddComponent<TeamDeathmatchMode>();
                break;

            case GameModeType.FreeForAll:
                Debug.LogWarning("[GameModeManager] FreeForAll aun no implementado");
                break;

            case GameModeType.CaptureTheFlag:
                Debug.LogWarning("[GameModeManager] CaptureTheFlag aun no implementado");
                break;

            default:
                Debug.LogError("[GameModeManager] Modo de juego no reconocido: " + gameModeType);
                return;
        }

        if (activeGameMode != null)
        {
            activeGameMode.Initialize(this, networkManager);
        }
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (activeGameMode != null)
        {
            activeGameMode.OnPlayerJoinedRoom(newPlayer);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (activeGameMode != null)
        {
            activeGameMode.OnPlayerLeftRoom(otherPlayer);
        }
    }

    public void NotifyPlayerKilled(PhotonView victim, PhotonView killer)
    {
        if (activeGameMode != null)
        {
            activeGameMode.OnPlayerKilled(victim, killer);
        }
    }

    public void NotifyPlayerRespawned(PhotonView player)
    {
        if (activeGameMode != null)
        {
            activeGameMode.OnPlayerRespawned(player);
        }
    }

    public Vector3 GetSpawnPointForPlayer(Player player)
    {
        if (activeGameMode != null)
        {
            return activeGameMode.GetSpawnPoint(player);
        }

        if (networkManager != null && networkManager.spawnPoint != null)
        {
            return networkManager.spawnPoint.position;
        }

        return Vector3.zero;
    }

    public bool CanPlayerRespawn(PhotonView player)
    {
        if (activeGameMode != null)
        {
            return activeGameMode.CanPlayerRespawn(player);
        }

        return true;
    }

    public GameModeBase GetActiveGameMode()
    {
        return activeGameMode;
    }

    public GameState GetCurrentGameState()
    {
        if (activeGameMode != null)
        {
            return activeGameMode.GetCurrentState();
        }

        return GameState.WaitingForPlayers;
    }

    public float GetCountdownTimer()
    {
        if (activeGameMode != null)
        {
            return activeGameMode.GetCountdownTimer();
        }

        return 0f;
    }

    public void OnFriendListUpdate(System.Collections.Generic.List<FriendInfo> friendList) { }
    public void OnCreatedRoom() { }
    public void OnCreateRoomFailed(short returnCode, string message) { }
    public void OnJoinedRoom() { }
    public void OnJoinRoomFailed(short returnCode, string message) { }
    public void OnJoinRandomFailed(short returnCode, string message) { }
    public void OnLeftRoom() { }
}

public enum GameModeType
{
    TeamDeathmatch,
    FreeForAll,
    CaptureTheFlag,
}
