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

        string teamAPlayers = "TEAM A:\n";
        string teamBPlayers = "TEAM B:\n";

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string playerName = string.IsNullOrEmpty(player.NickName)
                ? $"Player_{player.ActorNumber}"
                : player.NickName;

            if (player.CustomProperties.ContainsKey("Team"))
            {
                string team = (string)player.CustomProperties["Team"];

                if (team == "A")
                {
                    teamACount++;
                    teamAPlayers += playerName + "\n";
                }
                else if (team == "B")
                {
                    teamBCount++;
                    teamBPlayers += playerName + "\n";
                }
            }
        }

        // Actualizar los textos de UI
        if (teamAPlayersText != null)
        {
            teamAPlayersText.text = teamAPlayers + $"\n({teamACount} players)";
        }

        if (teamBPlayersText != null)
        {
            teamBPlayersText.text = teamBPlayers + $"\n({teamBCount} players)";
        }

        // Actualizar botón de start game
        if (startGameButton != null)
        {
            bool canStart = PhotonNetwork.IsMasterClient && (teamACount > 0 && teamBCount > 0);
            startGameButton.interactable = canStart;
        }
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
        Invoke("UpdateTeamDisplay", 0.5f);
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