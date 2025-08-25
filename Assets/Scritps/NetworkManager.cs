using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    void Awake()
    {
        // Singleton para que no se destruya al cambiar de escena
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Conectando a Photon...");
        }
    }

    public void JoinRandomRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinRandomRoom();
            Debug.Log("Intentando unirse a un Room...");
        }
        else
        {
            Debug.LogWarning("No estás conectado aún, no se puede unir a un Room.");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al Master Server.");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No había Room, creando uno nuevo...");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Entraste a un Room.");
        PhotonNetwork.LoadLevel("Prueba1"); // ?? IMPORTANTE: usa LoadLevel de Photon
    }
}
