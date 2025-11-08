using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

        // SIEMPRE visible, pero solo interactuable si eres host Y hay jugadores
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = false;
        }
    }

    void Update()
    {
        // Debug temporal - presiona F1 para ver info
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("=== DEBUG LOBBY ===");
            Debug.Log("IsMasterClient: " + PhotonNetwork.IsMasterClient);
            Debug.Log("IsConnected: " + PhotonNetwork.IsConnected);
            Debug.Log("InRoom: " + PhotonNetwork.InRoom);

            if (PhotonNetwork.InRoom)
            {
                Debug.Log("PlayerCount: " + PhotonNetwork.CurrentRoom.PlayerCount);

                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    string team = p.CustomProperties.ContainsKey("Team") ?
                        (string)p.CustomProperties["Team"] : "SIN EQUIPO";
                    Debug.Log("Player: " + p.NickName + " | Team: " + team + " | IsMaster: " + p.IsMasterClient);
                }
            }

            Debug.Log("StartButton interactable: " + startGameButton.interactable);
            Debug.Log("==================");
        }
    }

    public void OpenLobby()
    {
        lobbyPanel.SetActive(true);
        UpdateTeamDisplay();
        Debug.Log("Lobby abierto");
    }

    void JoinTeam(string team)
    {
        myTeam = team;

        Hashtable props = new Hashtable();
        props["Team"] = team;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log("Te uniste al Team " + team);

        // Deshabilitar botones después de elegir
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

        // Actualizar textos
        teamAPlayersText.text = "Team A (" + teamACount + "):\n" + string.Join("\n", teamAPlayers);
        teamBPlayersText.text = "Team B (" + teamBCount + "):\n" + string.Join("\n", teamBPlayers);

        // Actualizar estado del botón Start
        UpdateStartButton(teamACount, teamBCount);

        Debug.Log("Teams - A: " + teamACount + " | B: " + teamBCount + " | Master: " + PhotonNetwork.IsMasterClient);
    }

    void UpdateStartButton(int teamACount, int teamBCount)
    {
        if (startGameButton == null) return;

        bool isMaster = PhotonNetwork.IsMasterClient;
        bool teamsReady = (teamACount > 0 && teamBCount > 0);

        // El botón es interactuable SOLO si eres host Y hay al menos 1 jugador en cada equipo
        startGameButton.interactable = isMaster && teamsReady;

        // Cambiar el texto del botón según el estado
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

        Debug.Log("Botón Start actualizado - Interactuable: " + startGameButton.interactable);
    }

    void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Solo el host puede iniciar la partida");
            return;
        }

        // Verificar que hay jugadores en ambos equipos
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
            Debug.Log("Iniciando partida...");
            PhotonNetwork.LoadLevel(gameSceneName);
        }
        else
        {
            Debug.LogWarning("Se necesita al menos 1 jugador por equipo para iniciar");
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Team"))
        {
            Debug.Log(targetPlayer.NickName + " cambió de equipo");
            UpdateTeamDisplay();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Jugador entró: " + newPlayer.NickName);
        UpdateTeamDisplay();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Jugador salió: " + otherPlayer.NickName);
        UpdateTeamDisplay();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Nuevo host: " + newMasterClient.NickName);
        UpdateTeamDisplay();
    }
}