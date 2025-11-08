using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class TeamManager : MonoBehaviourPunCallbacks
{
    public static TeamManager Instance;

    [Header("Team Settings")]
    public int killsPerPlayer = 5;

    private int teamAKills = 0;
    private int teamBKills = 0;
    private int teamAPlayers = 0;
    private int teamBPlayers = 0;

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
        // Contar jugadores por equipo
        CountTeamPlayers();
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

        Debug.Log("Team A: " + teamAPlayers + " jugadores | Team B: " + teamBPlayers + " jugadores");
    }

    public void RegisterKill(string killerTeam)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (killerTeam == "A")
        {
            teamAKills++;
            Debug.Log("Team A Kills: " + teamAKills);
        }
        else if (killerTeam == "B")
        {
            teamBKills++;
            Debug.Log("Team B Kills: " + teamBKills);
        }

        // Sincronizar kills
        Hashtable props = new Hashtable();
        props["TeamAKills"] = teamAKills;
        props["TeamBKills"] = teamBKills;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        int targetKills = Mathf.Max(teamAPlayers, teamBPlayers) * killsPerPlayer;

        if (teamAKills >= targetKills)
        {
            photonView.RPC("AnnounceWinner", RpcTarget.All, "Team A");
        }
        else if (teamBKills >= targetKills)
        {
            photonView.RPC("AnnounceWinner", RpcTarget.All, "Team B");
        }
    }

    [PunRPC]
    void AnnounceWinner(string winnerTeam)
    {
        Debug.Log("¡" + winnerTeam + " ha ganado!");
        // Aquí puedes mostrar UI de victoria
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
}
