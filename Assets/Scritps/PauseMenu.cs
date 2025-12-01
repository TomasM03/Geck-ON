using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public Button resumeButton;
    public Button mainMenuButton;

    public string mainMenuScene = "MainMenu";

    private bool isPaused = false;
    private PlayerCamera playerCamera;
    private PlayerController playerController;
    private Weapon[] playerWeapons;
    private bool wasControlEnabled = true;

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        FindLocalPlayerComponents();
    }

    void FindLocalPlayerComponents()
    {
        PhotonView[] allViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in allViews)
        {
            if (pv.IsMine && pv.GetComponent<PlayerController>() != null)
            {
                playerCamera = pv.GetComponentInChildren<PlayerCamera>();
                playerController = pv.GetComponent<PlayerController>();
                playerWeapons = pv.GetComponentsInChildren<Weapon>();
                break;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (TeamManager.Instance != null && TeamManager.Instance.IsMatchEnded())
            {
                return;
            }

            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (playerCamera == null || playerController == null)
        {
            FindLocalPlayerComponents();
        }
    }

    public void Pause()
    {
        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerCamera != null)
        {
            playerCamera.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerWeapons != null)
        {
            foreach (Weapon weapon in playerWeapons)
            {
                if (weapon != null)
                {
                    weapon.enabled = false;
                }
            }
        }
    }

    public void Resume()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (playerWeapons != null)
        {
            foreach (Weapon weapon in playerWeapons)
            {
                if (weapon != null)
                {
                    weapon.enabled = true;
                }
            }
        }
    }

    public void GoToMainMenu()
    {
        isPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}