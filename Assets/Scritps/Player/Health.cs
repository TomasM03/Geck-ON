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

    [Header("Visual (Optional)")]
    public GameObject visualModel;

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
        if (isDead) return;

        isDead = true;

        if (photonView.IsMine)
        {
            if (killerViewID != -1)
            {
                PhotonView killerView = PhotonView.Find(killerViewID);
                if (killerView != null && killerView.Owner.CustomProperties.ContainsKey("Team"))
                {
                    string killerTeam = (string)killerView.Owner.CustomProperties["Team"];
                    string myTeam = "";

                    if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
                    {
                        myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
                    }

                    if (killerTeam != myTeam && TeamManager.Instance != null)
                    {
                        string teamNameFormatted = killerTeam == "A" ? "Team A" : "Team B";
                        TeamManager.Instance.RegisterKill(teamNameFormatted);
                    }
                }
            }

            if (playerController != null) playerController.enabled = false;
            if (playerCamera != null) playerCamera.UnlockCursor();
            if (deathPanel != null) deathPanel.SetActive(true);

            Invoke("Respawn", respawnDelay);
        }

        photonView.RPC("HidePlayer", RpcTarget.All);
    }

    [PunRPC]
    void HidePlayer()
    {
        if (visualModel != null)
        {
            visualModel.SetActive(false);
        }
        else
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                if (rend != null) rend.enabled = false;
            }
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

        photonView.RPC("ShowPlayer", RpcTarget.All);
    }

    [PunRPC]
    void ShowPlayer()
    {
        if (visualModel != null)
        {
            visualModel.SetActive(true);
        }
        else
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                if (rend != null) rend.enabled = true;
            }
        }
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