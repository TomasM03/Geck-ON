using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class TeamManager : MonoBehaviourPunCallbacks
{
    public static TeamManager Instance;

    [Header("Team Settings")]
    public int killsPerPlayer = 5;

    private int teamAKills = 0;
    private int teamBKills = 0;
    private int teamAPlayers = 0;
    private int teamBPlayers = 0;
    private bool matchEnded = false;

    public event Action<string> onMatchEnd;
    public event Action<string, int> onKillRegistered;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CountTeamPlayers();
        ResetMatch();
    }

    void CountTeamPlayers()
    {
        teamAPlayers = 0;
        teamBPlayers = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("Team"))
            {
                string team = (string)player.CustomProperties["Team"];
                if (team == "A") teamAPlayers++;
                else if (team == "B") teamBPlayers++;
            }
        }
    }

    void ResetMatch()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        teamAKills = 0;
        teamBKills = 0;
        matchEnded = false;

        Hashtable props = new Hashtable();
        props["TeamAKills"] = 0;
        props["TeamBKills"] = 0;
        props["MatchEnded"] = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public void RegisterKill(string killerTeam)
    {
        photonView.RPC("RegisterKillRPC", RpcTarget.MasterClient, killerTeam);
    }

    void CheckWinCondition()
    {
        if (matchEnded) return;

        int targetKills = Mathf.Max(teamAPlayers, teamBPlayers) * killsPerPlayer;

        if (teamAKills >= targetKills)
        {
            EndMatch("Team A");
        }
        else if (teamBKills >= targetKills)
        {
            EndMatch("Team B");
        }
    }

    void EndMatch(string winnerTeam)
    {
        if (matchEnded) return;

        matchEnded = true;

        Hashtable props = new Hashtable();
        props["MatchEnded"] = true;
        props["WinnerTeam"] = winnerTeam;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        photonView.RPC("AnnounceWinner", RpcTarget.All, winnerTeam);

        SubmitScoresToLeaderboard(winnerTeam);
    }
    void SubmitScoresToLeaderboard(string winnerTeam)
    {
        if (LootLockerManager.Instance == null) return;

        // Determinar si el jugador local ganó
        string myTeam = "";
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
        {
            myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        }

        bool didIWin = (myTeam == "A" && winnerTeam == "Team A") ||
                       (myTeam == "B" && winnerTeam == "Team B");

        // Calcular el score (puedes ajustar la fórmula)
        int myScore = 0;
        if (myTeam == "A")
        {
            myScore = teamAKills;
        }
        else if (myTeam == "B")
        {
            myScore = teamBKills;
        }

        // Bonus por ganar
        if (didIWin)
        {
            myScore += 10; // 10 puntos bonus por victoria
        }

        // Enviar al leaderboard
        LootLockerManager.Instance.SubmitScore(myScore, (success) =>
        {
            if (success)
            {
                Debug.Log($"Score enviado al leaderboard: {myScore}");
            }
        });
    }

    [PunRPC]
    void AnnounceWinner(string winnerTeam)
    {
        onMatchEnd?.Invoke(winnerTeam);
    }

    public override void OnRoomPropertiesUpdate(Hashtable props)
    {
        if (props.ContainsKey("TeamAKills"))
        {
            teamAKills = (int)props["TeamAKills"];
        }
        if (props.ContainsKey("TeamBKills"))
        {
            teamBKills = (int)props["TeamBKills"];
        }
        if (props.ContainsKey("MatchEnded"))
        {
            matchEnded = (bool)props["MatchEnded"];
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CountTeamPlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CountTeamPlayers();
    }

    public string GetPlayerTeam(Player player)
    {
        if (player.CustomProperties.ContainsKey("Team"))
        {
            return (string)player.CustomProperties["Team"];
        }
        return "";
    }

    [PunRPC]
    void RegisterKillRPC(string killerTeam)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (matchEnded) return;

        if (killerTeam == "A")
        {
            teamAKills++;
        }
        else if (killerTeam == "B")
        {
            teamBKills++;
        }

        Hashtable props = new Hashtable();
        props["TeamAKills"] = teamAKills;
        props["TeamBKills"] = teamBKills;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        onKillRegistered?.Invoke(killerTeam, killerTeam == "A" ? teamAKills : teamBKills);

        CheckWinCondition();
    }

    public int GetTeamAKills() { return teamAKills; }
    public int GetTeamBKills() { return teamBKills; }
    public int GetTargetKills() { return Mathf.Max(teamAPlayers, teamBPlayers) * killsPerPlayer; }
    public bool IsMatchEnded() { return matchEnded; }
    public int GetTeamAPlayers() { return teamAPlayers; }
    public int GetTeamBPlayers() { return teamBPlayers; }
}