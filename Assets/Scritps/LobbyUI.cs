using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class LobbyUI : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public GameObject lobbyPanel;
    public TMP_Text teamAPlayersText;
    public TMP_Text teamBPlayersText;
    public Button joinTeamAButton;
    public Button joinTeamBButton;
    public Button startGameButton;

    [Header("Settings")]
    public string gameSceneName = "GameScene";

    private string myTeam = "";

    void Start()
    {
        lobbyPanel.SetActive(false);

        joinTeamAButton.onClick.AddListener(() => JoinTeam("A"));
        joinTeamBButton.onClick.AddListener(() => JoinTeam("B"));
        startGameButton.onClick.AddListener(StartGame);

        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = false;
        }
    }

    public void OpenLobby()
    {
        lobbyPanel.SetActive(true);
        UpdateTeamDisplay();
    }

    void JoinTeam(string team)
    {
        myTeam = team;

        Hashtable props = new Hashtable();
        props["Team"] = team;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        joinTeamAButton.interactable = false;
        joinTeamBButton.interactable = false;

        UpdateTeamDisplay();
    }

    void UpdateTeamDisplay()
    {
        int teamACount = 0;
        int teamBCount = 0;

        List<string> teamAPlayers = new List<string>();
        List<string> teamBPlayers = new List<string>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("Team"))
            {
                string team = (string)player.CustomProperties["Team"];
                if (team == "A")
                {
                    teamACount++;
                    teamAPlayers.Add(player.NickName);
                }
                else if (team == "B")
                {
                    teamBCount++;
                    teamBPlayers.Add(player.NickName);
                }
            }
        }

        teamAPlayersText.text = "Team A (" + teamACount + "):\n" + string.Join("\n", teamAPlayers);
        teamBPlayersText.text = "Team B (" + teamBCount + "):\n" + string.Join("\n", teamBPlayers);

        UpdateStartButton(teamACount, teamBCount);
    }

    void UpdateStartButton(int teamACount, int teamBCount)
    {
        if (startGameButton == null) return;

        bool isMaster = PhotonNetwork.IsMasterClient;
        bool teamsReady = (teamACount > 0 && teamBCount > 0);

        startGameButton.interactable = isMaster && teamsReady;

        TMP_Text buttonText = startGameButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            if (!isMaster)
            {
                buttonText.text = "Esperando al Host...";
                buttonText.color = Color.gray;
            }
            else if (!teamsReady)
            {
                buttonText.text = "Necesitas 1v1 mínimo";
                buttonText.color = Color.yellow;
            }
            else
            {
                buttonText.text = "INICIAR PARTIDA";
                buttonText.color = Color.green;
            }
        }
    }

    void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        int teamACount = 0;
        int teamBCount = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("Team"))
            {
                string team = (string)player.CustomProperties["Team"];
                if (team == "A") teamACount++;
                else if (team == "B") teamBCount++;
            }
        }

        if (teamACount > 0 && teamBCount > 0)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Team"))
        {
            UpdateTeamDisplay();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateTeamDisplay();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateTeamDisplay();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdateTeamDisplay();
    }
}