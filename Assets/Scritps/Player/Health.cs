using UnityEngine;
using Photon.Pun;
using TMPro;

public class Health : MonoBehaviourPun
{
    public float maxHealth = 100f;
    private float currentHealth;

    public TMP_Text healthText;

    public GameObject deathPanel;
    public float deathScreenDelay = 2f;
    public float respawnDelay = 3f;

    private PlayerController playerController;
    private PlayerCamera playerCamera;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        playerCamera = GetComponentInChildren<PlayerCamera>();
        animator = GetComponentInChildren<Animator>();

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

        photonView.RPC("PlayDeathAnimationRPC", RpcTarget.All);

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
                        TeamManager.Instance.RegisterKill(killerTeam);
                    }
                }
            }

            if (playerController != null) playerController.enabled = false;

            Invoke("ShowDeathScreen", deathScreenDelay);
            Invoke("Respawn", deathScreenDelay + respawnDelay);
        }
    }

    [PunRPC]
    void PlayDeathAnimationRPC()
    {
        if (animator != null)
        {
            animator.Play("Death");
            animator.SetBool("IsDead", true);
        }
    }

    void ShowDeathScreen()
    {
        if (!photonView.IsMine) return;

        if (deathPanel != null) deathPanel.SetActive(true);
        if (playerCamera != null) playerCamera.UnlockCursor();
    }

    void Respawn()
    {
        if (!photonView.IsMine) return;

        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthUI();

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

        photonView.RPC("ResetToIdleRPC", RpcTarget.All);

        if (playerController != null) playerController.enabled = true;
        if (playerCamera != null) playerCamera.LockCursor();
        if (deathPanel != null) deathPanel.SetActive(false);
    }

    [PunRPC]
    void ResetToIdleRPC()
    {
        if (animator != null)
        {
            animator.Play("Idle", 0, 0f);
            animator.SetBool("IsDead", false);
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

    public bool IsDead()
    {
        return isDead;
    }
}