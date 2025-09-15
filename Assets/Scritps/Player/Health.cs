using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Health : MonoBehaviourPun
{
    //Sets
    public float maxHealth = 100f;
    public bool isPlayer = false;
    //DeadSets
    public string mainMenuScene = "MainMenu";
    public float deathDelay = 2f;

    //Health
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    //DamageSyst
    public void TakeDamage(float damage)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        currentHealth -= damage;

        photonView.RPC("SyncHealth", RpcTarget.Others, currentHealth);


        if (currentHealth <= 0)
        {
            photonView.RPC("SyncDeath", RpcTarget.All);
        }
    }

    [PunRPC]
    void ApplyDamage(float damage)
    {
        currentHealth -= damage;

        // Sincronizar HP
        photonView.RPC("SyncHealth", RpcTarget.Others, currentHealth);

        Debug.Log(currentHealth);

        if (currentHealth <= 0)
        {
            photonView.RPC("SyncDeath", RpcTarget.All);
        }
    }

    [PunRPC]
    void SyncHealth(float newHealth)
    {
        currentHealth = newHealth;
    }

    [PunRPC]
    void SyncDeath()
    {
        Die();
    }

    //DiedVoid
    void Die()
    {
        if (isPlayer)
        {
            PhotonView pv = GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                Debug.Log("Te mataron pete");
                StartCoroutine(ReturnToMainMenu());
            }
            else
            {
                Debug.Log("+1 Kill Anashe");
                gameObject.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //BackMenuNUM
    IEnumerator ReturnToMainMenu()
    {
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        PlayerCamera playerCam = GetComponentInChildren<PlayerCamera>();
        if (playerCam != null)
        {
            playerCam.DisableCamera();
        }

        yield return new WaitForSeconds(deathDelay);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        SceneManager.LoadScene(mainMenuScene);
    }

    //Percentage
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}