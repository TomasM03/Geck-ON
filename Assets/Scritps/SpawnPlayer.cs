using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpawnPlayer : MonoBehaviourPunCallbacks
{
    [Header("Player Settings")]
    public GameObject playerPrefab;
    public Transform spawnPoint;

    private void Start()
    {
        // Si ya estás dentro de una sala y la escena cargó, spawnea el jugador
        if (PhotonNetwork.InRoom)
        {
            SpawnPlayers();
        }
    }

    public override void OnJoinedRoom()
    {
        // Solo por seguridad, en caso de que se cargue la escena sin Start
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab no asignado en GameManager");
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        PhotonNetwork.Instantiate(playerPrefab.name, pos, Quaternion.identity);
    }
}
