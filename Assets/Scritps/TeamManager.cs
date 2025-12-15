using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class TeamManager : MonoBehaviourPunCallbacks
{
    public static TeamManager Instance;

    public int killsPerPlayer = 5;

    private int teamAKills = 0;
    private int teamBKills = 0;
    private int teamAPlayers = 0;
    private int teamBPlayers = 0;
    private bool matchEnded = false;

    public event Action<string> onMatchEnd;
    public event Action<string, int> onKillRegistered;
    public event Action<string, int, int> onPlayerDisconnected;
    public event Action<int> onTargetKillsChanged;

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
        ResetMatch();
        CountTeamPlayers();
    }

    public void CountTeamPlayers()
    {
        int previousTeamA = teamAPlayers;
        int previousTeamB = teamBPlayers;

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

        if (previousTeamA != teamAPlayers || previousTeamB != teamBPlayers)
        {
            int newTarget = GetTargetKills();
            onTargetKillsChanged?.Invoke(newTarget);
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

        int targetKills = GetTargetKills();

        if (teamAKills >= targetKills && targetKills > 0)
        {
            EndMatch("Team A");
        }
        else if (teamBKills >= targetKills && targetKills > 0)
        {
            EndMatch("Team B");
        }
    }

    void CheckEmptyTeam()
    {
        if (matchEnded) return;

        if (teamAPlayers == 0 && teamBPlayers > 0)
        {
            EndMatch("Team B");
        }
        else if (teamBPlayers == 0 && teamAPlayers > 0)
        {
            EndMatch("Team A");
        }
        else if (teamAPlayers == 0 && teamBPlayers == 0)
        {
            EndMatch("Draw");
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

        string myTeam = "";
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
        {
            myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        }

        bool didIWin = (myTeam == "A" && winnerTeam == "Team A") ||
                       (myTeam == "B" && winnerTeam == "Team B");

        int myScore = 0;
        if (myTeam == "A")
        {
            myScore = teamAKills;
        }
        else if (myTeam == "B")
        {
            myScore = teamBKills;
        }

        if (didIWin)
        {
            myScore += 10;
        }

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
        matchEnded = true;
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
        string disconnectedTeam = "";
        if (otherPlayer.CustomProperties.ContainsKey("Team"))
        {
            disconnectedTeam = (string)otherPlayer.CustomProperties["Team"];
        }

        CountTeamPlayers();

        onPlayerDisconnected?.Invoke(disconnectedTeam, teamAPlayers, teamBPlayers);

        if (PhotonNetwork.IsMasterClient && !matchEnded)
        {
            CheckEmptyTeam();
            CheckWinCondition();
        }
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