using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RespawnCanvas : MonoBehaviour
{
    public Button respawnButton;
    public Button mainMenuButton;

    public GameObject deathPanel;

    void Start()
    {
        respawnButton.onClick.AddListener(Respawn);
        mainMenuButton.onClick.AddListener(MainMenu);
        deathPanel.SetActive(false);
    }

    //Respawn Button
    public void Respawn()
    {
        PhotonView[] players = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in players)
        {
            if (pv.IsMine)
            {
                Health health = pv.GetComponent<Health>();
                if (health != null)
                {
                    health.Respawn();
                    deathPanel.SetActive(false);
                    break;
                }
            }
        }
    }

    //MainMenu Button
    public void MainMenu()
    {
        PhotonView[] players = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in players)
        {
            if (pv.IsMine)
            {
                Health health = pv.GetComponent<Health>();
                if (health != null)
                {
                    StartCoroutine(ReturnToMainMenu());
                    break;
                }
            }
        }
    }

    //BackMenuNUM
    IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(0.5f);
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
        SceneManager.LoadScene("MainMenu");
    }
}
