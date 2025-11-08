using UnityEngine;
using Photon.Pun;
using TMPro;

public class Health : MonoBehaviourPun
{
    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public TMP_Text healthText;

    [Header("Death")]
    public GameObject deathPanel;
    public float respawnDelay = 3f;

    private PlayerController playerController;
    private PlayerCamera playerCamera;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        playerCamera = GetComponentInChildren<PlayerCamera>();

        UpdateHealthUI();

        if (!photonView.IsMine && healthText != null)
        {
            healthText.gameObject.SetActive(false);
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }

    [PunRPC]
    void TakeDamageRPC(float damage, int shooterID)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die(shooterID);
        }
    }

    public void TakeDamage(float damage, PhotonView shooter)
    {
        int shooterID = shooter != null ? shooter.ViewID : -1;
        photonView.RPC("TakeDamageRPC", RpcTarget.All, damage, shooterID);
    }

    void Die(int killerViewID)
    {
        isDead = true;

        if (photonView.IsMine)
        {
            if (killerViewID != -1)
            {
                PhotonView killerView = PhotonView.Find(killerViewID);
                if (killerView != null && killerView.Owner.CustomProperties.ContainsKey("Team"))
                {
                    string killerTeam = (string)killerView.Owner.CustomProperties["Team"];

                    if (TeamManager.Instance != null)
                    {
                        TeamManager.Instance.RegisterKill(killerTeam);
                    }
                }
            }

            if (playerController != null) playerController.enabled = false;
            if (playerCamera != null) playerCamera.UnlockCursor();

            if (deathPanel != null) deathPanel.SetActive(true);

            Invoke("Respawn", respawnDelay);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Respawn()
    {
        if (!photonView.IsMine) return;

        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (playerController != null) playerController.enabled = true;
        if (playerCamera != null) playerCamera.LockCursor();

        if (deathPanel != null) deathPanel.SetActive(false);

        NetworkManager netManager = FindObjectOfType<NetworkManager>();
        if (netManager != null)
        {
            string myTeam = "";
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
            {
                myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            }
            transform.position = netManager.GetSpawnPosition(myTeam);
        }

        photonView.RPC("SyncRespawn", RpcTarget.Others);
    }

    [PunRPC]
    void SyncRespawn()
    {
        gameObject.SetActive(true);
        isDead = false;
    }

    void UpdateHealthUI()
    {
        if (healthText != null && photonView.IsMine)
        {
            healthText.text = "HP: " + Mathf.CeilToInt(currentHealth) + "%";

            if (currentHealth > 65)
                healthText.color = Color.green;
            else if (currentHealth > 35)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.red;
        }
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}