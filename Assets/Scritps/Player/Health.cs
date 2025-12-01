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
    [Tooltip("Tiempo hasta que aparece la pantalla de muerte")]
    public float deathScreenDelay = 2f;
    [Tooltip("Tiempo hasta respawn después de la pantalla de muerte")]
    public float respawnDelay = 3f;

    [Header("Visual (Optional)")]
    public GameObject visualModel;

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

        // Activar animación de muerte en todos los clientes inmediatamente
        photonView.RPC("PlayDeathAnimationRPC", RpcTarget.All);

        if (photonView.IsMine)
        {
            // Registrar kill
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

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
}