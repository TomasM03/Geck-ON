using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public abstract class GameModeBase : MonoBehaviour
{
    [Header("Game Mode Settings")]
    public string modeName = "Game Mode";
    public bool showLogs = true;

    [Header("Match Settings")]
    public int minPlayersToStart = 2;
    public float countdownDuration = 5f;

    protected GameState currentState = GameState.WaitingForPlayers;
    protected float countdownTimer = 0f;

    protected GameModeManager gameModeManager;
    protected NetworkManager networkManager;

    public virtual void Initialize(GameModeManager manager, NetworkManager netManager)
    {
        gameModeManager = manager;
        networkManager = netManager;

        if (showLogs)
            Debug.Log("[" + modeName + "] Inicializando modo de juego");

        SyncStateFromRoom();
        OnModeInitialized();
    }

    protected virtual void OnModeInitialized() { }

    public void SyncStateFromRoom()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState"))
        {
            int stateValue = (int)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];
            currentState = (GameState)stateValue;

            if (showLogs)
                Debug.Log("[" + modeName + "] Estado sincronizado desde Room: " + currentState);
        }
    }

    public virtual void UpdateGameMode()
    {
        switch (currentState)
        {
            case GameState.WaitingForPlayers:
                UpdateWaitingForPlayers();
                break;

            case GameState.Countdown:
                UpdateCountdown();
                break;

            case GameState.InProgress:
                UpdateInProgress();
                break;

            case GameState.GameOver:
                UpdateGameOver();
                break;
        }
    }

    protected virtual void UpdateWaitingForPlayers()
    {
        if (CanMatchStart())
        {
            ChangeState(GameState.Countdown);
        }
    }

    protected virtual void UpdateCountdown()
    {
        countdownTimer -= Time.deltaTime;

        if (countdownTimer <= 0f)
        {
            StartMatch();
        }

        if (!CanMatchStart())
        {
            ChangeState(GameState.WaitingForPlayers);
        }
    }

    protected virtual void UpdateInProgress()
    {
        CheckWinCondition();
    }

    protected virtual void UpdateGameOver() { }

    protected virtual void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        if (showLogs)
            Debug.Log("[" + modeName + "] Estado: " + currentState + " -> " + newState);

        currentState = newState;

        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["GameState"] = (int)newState;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        OnStateChanged(newState);

        switch (newState)
        {
            case GameState.Countdown:
                countdownTimer = countdownDuration;
                OnCountdownStarted();
                break;

            case GameState.InProgress:
                OnMatchStarted();
                break;

            case GameState.GameOver:
                OnMatchEnded();
                break;
        }
    }

    protected virtual void OnStateChanged(GameState newState) { }

    public virtual void OnPlayerJoinedRoom(Player newPlayer)
    {
        if (showLogs)
            Debug.Log("[" + modeName + "] Jugador unido: " + newPlayer.NickName);

        OnPlayerJoined(newPlayer);
    }

    public virtual void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (showLogs)
            Debug.Log("[" + modeName + "] Jugador salio: " + otherPlayer.NickName);

        OnPlayerLeft(otherPlayer);
    }

    protected virtual void OnPlayerJoined(Player player) { }
    protected virtual void OnPlayerLeft(Player player) { }

    public virtual void OnPlayerKilled(PhotonView victimView, PhotonView killerView)
    {
        if (currentState != GameState.InProgress)
            return;

        string victimName = victimView != null ? victimView.Owner.NickName : "Unknown";
        string killerName = killerView != null ? killerView.Owner.NickName : "Unknown";

        if (showLogs)
            Debug.Log("[" + modeName + "] " + killerName + " elimino a " + victimName);

        ProcessKill(victimView, killerView);
    }

    protected abstract void ProcessKill(PhotonView victim, PhotonView killer);

    public virtual Vector3 GetSpawnPoint(Player player)
    {
        if (networkManager != null && networkManager.spawnPoint != null)
        {
            Vector3 basePos = networkManager.spawnPoint.position;
            return basePos + new Vector3(
                Random.Range(-2f, 2f),
                0,
                Random.Range(-2f, 2f)
            );
        }

        return Vector3.zero;
    }

    public virtual void OnPlayerRespawned(PhotonView playerView)
    {
        if (showLogs)
            Debug.Log("[" + modeName + "] Jugador respawneado: " + playerView.Owner.NickName);
    }

    public virtual bool CanPlayerRespawn(PhotonView playerView)
    {
        return currentState == GameState.InProgress;
    }

    public virtual bool CanMatchStart()
    {
        return PhotonNetwork.CurrentRoom != null &&
               PhotonNetwork.CurrentRoom.PlayerCount >= minPlayersToStart;
    }

    protected virtual void StartMatch()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (showLogs)
            Debug.Log("[" + modeName + "] Partida iniciada!");

        ChangeState(GameState.InProgress);
    }

    protected abstract void CheckWinCondition();

    protected virtual void EndMatch(string winnerInfo)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (showLogs)
            Debug.Log("[" + modeName + "] Partida terminada. Ganador: " + winnerInfo);

        ChangeState(GameState.GameOver);
    }

    protected virtual void OnCountdownStarted() { }
    protected virtual void OnMatchStarted() { }
    protected virtual void OnMatchEnded() { }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    public float GetCountdownTimer()
    {
        return countdownTimer;
    }

    public string GetModeName()
    {
        return modeName;
    }
}

public enum GameState
{
    WaitingForPlayers,
    Countdown,
    InProgress,
    GameOver
}