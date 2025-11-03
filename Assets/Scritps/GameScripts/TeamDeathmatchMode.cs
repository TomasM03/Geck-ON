using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TeamDeathmatchMode : GameModeBase
{
    [Header("Team Deathmatch Settings")]
    public int killsPerPlayer = 10;

    private int teamAKills = 0;
    private int teamBKills = 0;
    private int targetKills = 2;

    protected override void OnModeInitialized()
    {
        base.OnModeInitialized();

        modeName = "Team Deathmatch";

        if (showLogs)
            Debug.Log("[TDM] Modo Team Deathmatch inicializado (placeholder)");

        CalculateTargetKills();
    }

    protected override void OnPlayerJoined(Player player)
    {
        base.OnPlayerJoined(player);

        CalculateTargetKills();
    }

    protected override void OnPlayerLeft(Player player)
    {
        base.OnPlayerLeft(player);

        CalculateTargetKills();
    }

    protected override void ProcessKill(PhotonView victim, PhotonView killer)
    {
        if (killer == null || victim == null)
            return;

        teamAKills++;

        if (showLogs)
            Debug.Log("[TDM] Team A: " + teamAKills + " | Team B: " + teamBKills + " | Target: " + targetKills);
    }

    protected override void CheckWinCondition()
    {
        if (teamAKills >= targetKills)
        {
            EndMatch("Team A");
        }
        else if (teamBKills >= targetKills)
        {
            EndMatch("Team B");
        }
    }

    void CalculateTargetKills()
    {
 //       if (PhotonNetwork.CurrentRoom == null)
   //         return;

     //   int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
       // targetKills = totalPlayers * killsPerPlayer;

        //if (showLogs)
          //  Debug.Log("[TDM] Jugadores: " + totalPlayers + " | Kills objetivo: " + targetKills);
    }

    protected override void OnMatchStarted()
    {
        base.OnMatchStarted();

        teamAKills = 0;
        teamBKills = 0;

        if (showLogs)
            Debug.Log("[TDM] Partida iniciada! Kills objetivo: " + targetKills);
    }

    protected override void OnMatchEnded()
    {
        base.OnMatchEnded();

        if (showLogs)
            Debug.Log("[TDM] Partida terminada");
    }

    public int GetTeamAKills() { return teamAKills; }
    public int GetTeamBKills() { return teamBKills; }
    public int GetTargetKills() { return targetKills; }
}
