using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

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

    public TMP_Text healthTxt;

    void Start()
    {
        currentHealth = maxHealth;
    }

    //DamageSyst
    public void TakeDamage(float damage)
    {
        //Si es jugador
        if (isPlayer)
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


            healthTxt.text = currentHealth.ToString() + "%";

            if (currentHealth < 60)
            {
                healthTxt.color = Color.yellow;
            }
            else if (currentHealth < 30)
            {
                healthTxt.color = Color.red;
            }
        }
        else //Si es objeto
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Destroy(gameObject);
            }
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
                // Desactivar jugador pero NO salir de la escena
                PlayerController playerController = GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.enabled = false;
                }

                PlayerCamera playerCam = GetComponentInChildren<PlayerCamera>();
                if (playerCam != null)
                {
                    playerCam.UnlockCursor();
                }

                RespawnCanvas respawnCanvas = FindObjectOfType<RespawnCanvas>();
                Debug.Log(respawnCanvas);
                respawnCanvas.deathPanel.SetActive(true);

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

    //RespawnVoid
    public void Respawn()
    {
        if (!photonView.IsMine) return;

        currentHealth = maxHealth;

        // Reactivar jugador
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        PlayerCamera playerCam = GetComponentInChildren<PlayerCamera>();
        if (playerCam != null)
        {
            playerCam.LockCursor();
        }

        // Mover a posición de spawn
        NetworkManager networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager != null && networkManager.spawnPoint != null)
        {
            Vector3 spawnPos = networkManager.spawnPoint.position;
            spawnPos += new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            transform.position = spawnPos;
        }

        photonView.RPC("SyncRespawn", RpcTarget.Others);
    }

    [PunRPC]
    void SyncRespawn()
    {
        gameObject.SetActive(true);
        currentHealth = maxHealth;
    }


    //Percentage
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}