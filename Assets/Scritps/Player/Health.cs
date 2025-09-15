using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Health : MonoBehaviour
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
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    //DiedVoid
    void Die()
    {
        if (isPlayer)
        {
            Debug.Log("Player died - Returning to main menu");
            StartCoroutine(ReturnToMainMenu());
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