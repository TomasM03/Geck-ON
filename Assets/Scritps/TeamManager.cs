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
        if (!PhotonNetwork.IsMasterClient) return;
        if (matchEnded) return;

        if (killerTeam == "A")
        {
            teamAKills++;
            Debug.Log("TeamAKill");
        }
        else if (killerTeam == "B")
        {
            teamBKills++;
            Debug.Log("TeamBKill");
        }

        Hashtable props = new Hashtable();
        props["TeamAKills"] = teamAKills;
        props["TeamBKills"] = teamBKills;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        int currentKills = killerTeam == "A" ? teamAKills : teamBKills;
        onKillRegistered?.Invoke(killerTeam, currentKills);

        CheckWinCondition();
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

        if (GetComponent<PhotonView>() != null)
        {
            GetComponent<PhotonView>().RPC("AnnounceWinner", RpcTarget.All, winnerTeam);
        }
        else
        {
            onMatchEnd?.Invoke(winnerTeam);
        }
    }

    [PunRPC]
    void AnnounceWinner(string winnerTeam)
    {
        matchEnded = true;
        onMatchEnd?.Invoke(winnerTeam);
    }

    public override void OnRoomPropertiesUpdate(Hashtable props)
    {
        bool updated = false;

        if (props.ContainsKey("TeamAKills"))
        {
            int newKills = (int)props["TeamAKills"];
            if (newKills != teamAKills)
            {
                teamAKills = newKills;
                updated = true;
            }
        }

        if (props.ContainsKey("TeamBKills"))
        {
            int newKills = (int)props["TeamBKills"];
            if (newKills != teamBKills)
            {
                teamBKills = newKills;
                updated = true;
            }
        }

        if (props.ContainsKey("MatchEnded"))
        {
            bool ended = (bool)props["MatchEnded"];
            if (ended && !matchEnded)
            {
                matchEnded = true;

                if (props.ContainsKey("WinnerTeam"))
                {
                    string winner = (string)props["WinnerTeam"];
                    onMatchEnd?.Invoke(winner);
                }
            }
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

    public int GetTeamAKills() { return teamAKills; }
    public int GetTeamBKills() { return teamBKills; }
    public int GetTargetKills() { return Mathf.Max(teamAPlayers, teamBPlayers) * killsPerPlayer; }
    public bool IsMatchEnded() { return matchEnded; }
    public int GetTeamAPlayers() { return teamAPlayers; }
    public int GetTeamBPlayers() { return teamBPlayers; }
}