using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class DeathMatchUI : MonoBehaviour
{
    [Header("Score Display")]
    public TMP_Text teamAScoreText;
    public TMP_Text teamBScoreText;
    public TMP_Text targetScoreText;

    [Header("Victory Panel")]
    public GameObject victoryPanel;
    public TMP_Text victoryText;
    public TMP_Text victorySubtext;
    public Button returnToLobbyButton;

    [Header("Match Info")]
    public TMP_Text matchTimerText;
    public TMP_Text playerCountText;

    [Header("Settings")]
    public string lobbySceneName = "MainMenu";
    public float autoReturnDelay = 10f;

    private bool matchEnded = false;
    private float matchTime = 0f;

    void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (returnToLobbyButton != null)
        {
            returnToLobbyButton.onClick.AddListener(ReturnToLobby);
        }

        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.onMatchEnd += HandleMatchEnd;
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
    }

    void UpdateScoreDisplay()
    {
        if (TeamManager.Instance == null) return;

        int teamAKills = TeamManager.Instance.GetTeamAKills();
        int teamBKills = TeamManager.Instance.GetTeamBKills();
        int targetKills = TeamManager.Instance.GetTargetKills();

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
            targetScoreText.text = "First to " + targetKills;
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

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (victoryText != null)
        {
            victoryText.text = winnerTeam + " WINS!";

            if (winnerTeam == "Team A")
                victoryText.color = new Color(0.3f, 0.6f, 1f);
            else if (winnerTeam == "Team B")
                victoryText.color = new Color(1f, 0.3f, 0.3f);
        }

        if (victorySubtext != null)
        {
            string myTeam = "";
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
            {
                myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            }

            if ((myTeam == "A" && winnerTeam == "Team A") ||
                (myTeam == "B" && winnerTeam == "Team B"))
            {
                victorySubtext.text = "VICTORY!";
                victorySubtext.color = Color.green;
            }
            else
            {
                victorySubtext.text = "DEFEAT";
                victorySubtext.color = Color.red;
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
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }

        SceneManager.LoadScene(lobbySceneName);
    }

    void OnDestroy()
    {
        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.onMatchEnd -= HandleMatchEnd;
        }
    }
}