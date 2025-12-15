using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class InteractionUIManager : MonoBehaviour
{
    public TMP_Text interactionPrompt;

    public float interactionRange = 3f;
    public LayerMask interactionLayer;

    private Camera playerCamera;
    private PhotonView localPlayerPV;
    private string localPlayerTeam = "";
    private CoopDoorButton currentButton = null;

    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }

        StartCoroutine(FindLocalPlayer());
    }

    System.Collections.IEnumerator FindLocalPlayer()
    {
        while (localPlayerPV == null)
        {
            PhotonView[] views = FindObjectsOfType<PhotonView>();
            foreach (PhotonView pv in views)
            {
                if (pv.IsMine && pv.GetComponent<PlayerController>() != null)
                {
                    localPlayerPV = pv;

                    if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
                    {
                        localPlayerTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
                    }

                    PlayerCamera playerCam = pv.GetComponentInChildren<PlayerCamera>();
                    if (playerCam != null && playerCam.mainCam != null)
                    {
                        playerCamera = playerCam.mainCam;
                    }
                    break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        if (localPlayerPV == null || playerCamera == null)
            return;

        CheckInteraction();
        HandleInteractionInput();
    }

    void CheckInteraction()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            CoopDoorButton button = hit.collider.GetComponent<CoopDoorButton>();

            if (button != null && button.CanInteract(localPlayerPV.transform.position, localPlayerPV.transform.forward))
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

    void HandleInteractionInput()
    {
        if (currentButton != null && Input.GetKeyDown(KeyCode.E))
        {
            currentButton.ActivateButton(localPlayerTeam, localPlayerPV.ViewID);
            HidePrompt();
            currentButton = null;
        }
    }

    void ShowPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(true);
        }
    }

    void HidePrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }
    }
}