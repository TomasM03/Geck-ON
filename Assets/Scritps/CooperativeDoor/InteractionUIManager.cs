using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class InteractionUIManager : MonoBehaviour
{
    [Header("UI")]
    public Image interactionPromptImage;

    [Header("Raycast")]
    public float interactionRange = 5f;
    public LayerMask buttonLayer = ~0;

    private Camera playerCamera;
    private PhotonView photonView;
    private string localPlayerTeam = "";
    private CoopDoorButton currentButton = null;

    void Start()
    {
        photonView = GetComponentInParent<PhotonView>();

        if (photonView == null || !photonView.IsMine)
        {
            if (interactionPromptImage != null)
                interactionPromptImage.gameObject.SetActive(false);

            enabled = false;
            return;
        }

        if (interactionPromptImage != null)
            interactionPromptImage.gameObject.SetActive(false);

        PlayerCamera playerCam = GetComponentInChildren<PlayerCamera>();
        if (playerCam != null && playerCam.mainCam != null)
        {
            playerCamera = playerCam.mainCam;
        }

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
        {
            localPlayerTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        }

        Debug.Log($"InteractionUIManager initialized - Team: {localPlayerTeam}");
    }

    void Update()
    {
        if (playerCamera == null) return;

        CheckForButton();
        HandleInput();
    }

    void CheckForButton()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray.origin, ray.direction, out hit, interactionRange, buttonLayer))
        {
            CoopDoorButton button = hit.collider.GetComponent<CoopDoorButton>();

            if (button != null && !button.IsActivated() && button.GetRequiredTeam() == localPlayerTeam)
            {
                if (currentButton != button)
                {
                    currentButton = button;
                    ShowPrompt();
                }
                return;
            }
        }

        if (currentButton != null)
        {
            currentButton = null;
            HidePrompt();
        }
    }

    void HandleInput()
    {
        if (currentButton != null && Input.GetKeyDown(KeyCode.E))
        {
            currentButton.TryActivate(localPlayerTeam, photonView.ViewID);
            HidePrompt();
            currentButton = null;
        }
    }

    void ShowPrompt()
    {
        if (interactionPromptImage != null)
        {
            interactionPromptImage.gameObject.SetActive(true);
        }
    }

    void HidePrompt()
    {
        if (interactionPromptImage != null)
        {
            interactionPromptImage.gameObject.SetActive(false);
        }
    }
}