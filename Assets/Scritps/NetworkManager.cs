using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Player")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] teamASpawns;
    public Transform[] teamBSpawns;

    private bool hasSpawned = false;

    void Start()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            if (!PhotonNetwork.IsConnected)
            {
                ConfigurePhoton();
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            if (GameManager.Instance != null)
            {
                string savedNick = GameManager.Instance.GetNickname();
                if (!string.IsNullOrEmpty(savedNick) && PhotonNetwork.NickName != savedNick)
                {
                    PhotonNetwork.NickName = savedNick;
                }
            }

            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && !hasSpawned)
            {
                SpawnPlayer();
            }
        }
    }

    void ConfigurePhoton()
    {
        if (GameManager.Instance != null)
        {
            string savedNick = GameManager.Instance.GetNickname();
            if (!string.IsNullOrEmpty(savedNick))
            {
                PhotonNetwork.NickName = savedNick;
            }
            else
            {
                PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
            }
        }
        else
        {
            PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
        }

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al Master Server");
    }

    public override void OnJoinedRoom()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName != "MainMenu" && !hasSpawned)
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        if (hasSpawned) return;
        if (playerPrefab == null) return;

        string myTeam = "";
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
        {
            myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        }
        else
        {
            return;
        }

        Vector3 spawnPos = GetSpawnPosition(myTeam);
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
        hasSpawned = true;
    }

    public Vector3 GetSpawnPosition(string team)
    {
        Transform[] spawns = null;

        if (team == "A" && teamASpawns != null && teamASpawns.Length > 0)
        {
            spawns = teamASpawns;
        }
        else if (team == "B" && teamBSpawns != null && teamBSpawns.Length > 0)
        {
            spawns = teamBSpawns;
        }

        if (spawns != null && spawns.Length > 0)
        {
            Transform selectedSpawn = spawns[Random.Range(0, spawns.Length)];
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            return selectedSpawn.position + offset;
        }

        return new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        hasSpawned = false;
    }

    public override void OnLeftRoom()
    {
        hasSpawned = false;
    }
}