using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class DeathMatchUI : MonoBehaviourPunCallbacks
{
    public TMP_Text teamAScoreText;
    public TMP_Text teamBScoreText;
    public TMP_Text targetScoreText;

    public GameObject victoryPanel;
    public TMP_Text victoryText;
    public TMP_Text victorySubtext;
    public Button returnToLobbyButton;

    public TMP_Text matchTimerText;
    public TMP_Text playerCountText;

    public TMP_Text disconnectNotificationText;
    public float notificationDuration = 3f;

    public string mainMenuScene = "MainMenu";
    public float autoReturnDelay = 10f;

    private bool matchEnded = false;
    private float matchTime = 0f;
    private float notificationTimer = 0f;

    void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (disconnectNotificationText != null)
        {
            disconnectNotificationText.gameObject.SetActive(false);
        }

        if (returnToLobbyButton != null)
        {
            returnToLobbyButton.onClick.AddListener(ReturnToLobby);
        }

        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.onMatchEnd += HandleMatchEnd;
            TeamManager.Instance.onPlayerDisconnected += HandlePlayerDisconnected;
            TeamManager.Instance.onTargetKillsChanged += HandleTargetKillsChanged;
        }
    }

    void Update()
    {
        if (!matchEnded)
        {
            matchTime += Time.deltaTime;
            UpdateScoreDisplay();
            UpdateMatchInfo();
        }

        if (notificationTimer > 0)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0 && disconnectNotificationText != null)
            {
                disconnectNotificationText.gameObject.SetActive(false);
            }
        }
    }

    void HandlePlayerDisconnected(string team, int teamACount, int teamBCount)
    {
        if (disconnectNotificationText != null)
        {
            string teamName = team == "A" ? "Team A" : "Team B";
            int newTarget = TeamManager.Instance.GetTargetKills();

            if (teamACount == 0 || teamBCount == 0)
            {
                disconnectNotificationText.text = $"¡Jugador de {teamName} abandonó!\n¡Victoria por abandono!";
            }
            else
            {
                disconnectNotificationText.text = $"¡Jugador de {teamName} abandonó!\nNuevo objetivo: {newTarget} kills";
            }

            disconnectNotificationText.color = team == "A" ? new Color(0.3f, 0.6f, 1f) : new Color(1f, 0.3f, 0.3f);
            disconnectNotificationText.gameObject.SetActive(true);
            notificationTimer = notificationDuration;
        }

        UpdateScoreDisplay();
    }

    void HandleTargetKillsChanged(int newTarget)
    {
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (TeamManager.Instance == null) return;

        int teamAKills = TeamManager.Instance.GetTeamAKills();
        int teamBKills = TeamManager.Instance.GetTeamBKills();
        int targetKills = TeamManager.Instance.GetTargetKills();

        if (targetKills == 0) targetKills = 1;

        if (teamAScoreText != null)
        {
            teamAScoreText.text = "TEAM A: " + teamAKills + " / " + targetKills;

            float progress = (float)teamAKills / targetKills;
            if (progress >= 1f)
                teamAScoreText.color = Color.green;
            else if (progress >= 0.7f)
                teamAScoreText.color = Color.yellow;
            else
                teamAScoreText.color = new Color(0.3f, 0.6f, 1f);
        }

        if (teamBScoreText != null)
        {
            teamBScoreText.text = "TEAM B: " + teamBKills + " / " + targetKills;

            float progress = (float)teamBKills / targetKills;
            if (progress >= 1f)
                teamBScoreText.color = Color.green;
            else if (progress >= 0.7f)
                teamBScoreText.color = Color.yellow;
            else
                teamBScoreText.color = new Color(1f, 0.3f, 0.3f);
        }

        if (targetScoreText != null)
        {
            int teamAPlayers = TeamManager.Instance.GetTeamAPlayers();
            int teamBPlayers = TeamManager.Instance.GetTeamBPlayers();
            targetScoreText.text = $"First to {targetKills} ({teamAPlayers}v{teamBPlayers})";
        }
    }

    void UpdateMatchInfo()
    {
        if (matchTimerText != null)
        {
            int minutes = Mathf.FloorToInt(matchTime / 60f);
            int seconds = Mathf.FloorToInt(matchTime % 60f);
            matchTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (playerCountText != null && PhotonNetwork.InRoom)
        {
            playerCountText.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount;
        }
    }

    void HandleMatchEnd(string winnerTeam)
    {
        if (matchEnded) return;

        matchEnded = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (victoryText != null)
        {
            if (winnerTeam == "Draw")
            {
                victoryText.text = "EMPATE";
                victoryText.color = Color.gray;
            }
            else
            {
                victoryText.text = winnerTeam + " WINS!";

                if (winnerTeam == "Team A")
                    victoryText.color = new Color(0.3f, 0.6f, 1f);
                else if (winnerTeam == "Team B")
                    victoryText.color = new Color(1f, 0.3f, 0.3f);
            }
        }

        if (victorySubtext != null)
        {
            if (winnerTeam == "Draw")
            {
                victorySubtext.text = "Todos abandonaron";
                victorySubtext.color = Color.gray;
            }
            else
            {
                string myTeam = "";
                if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
                {
                    myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
                }

                bool enemyTeamEmpty = (myTeam == "A" && TeamManager.Instance.GetTeamBPlayers() == 0) ||
                                      (myTeam == "B" && TeamManager.Instance.GetTeamAPlayers() == 0);

                if ((myTeam == "A" && winnerTeam == "Team A") ||
                    (myTeam == "B" && winnerTeam == "Team B"))
                {
                    if (enemyTeamEmpty)
                    {
                        victorySubtext.text = "¡VICTORIA POR ABANDONO!";
                    }
                    else
                    {
                        victorySubtext.text = "¡VICTORIA!";
                    }
                    victorySubtext.color = Color.green;
                }
                else
                {
                    victorySubtext.text = "DERROTA";
                    victorySubtext.color = Color.red;
                }
            }
        }

        Invoke("AutoReturnToLobby", autoReturnDelay);
    }

    void AutoReturnToLobby()
    {
        ReturnToLobby();
    }

    void ReturnToLobby()
    {
        Time.timeScale = 1f;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;

        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.onMatchEnd -= HandleMatchEnd;
            TeamManager.Instance.onPlayerDisconnected -= HandlePlayerDisconnected;
            TeamManager.Instance.onTargetKillsChanged -= HandleTargetKillsChanged;
        }
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}