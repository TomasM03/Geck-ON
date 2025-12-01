using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class CoopDoorButton : MonoBehaviour
{
    public Renderer buttonRenderer;
    public GameObject interactPrompt;

    public Color inactiveColor = Color.red;
    public Color activeColor = Color.green;
    public Color waitingColor = Color.yellow;

    private CoopDoor parentDoor;
    private int buttonIndex;
    private bool playerInRange = false;
    private bool isPressed = false;
    private PhotonView localPlayerPV;
    private string localPlayerTeam = "";

    private Material buttonMaterial;

    void Start()
    {
        if (buttonRenderer == null)
        {
            buttonRenderer = GetComponent<Renderer>();
        }

        if (buttonRenderer != null)
        {
            buttonMaterial = new Material(buttonRenderer.material);
            buttonRenderer.material = buttonMaterial;
            buttonMaterial.color = inactiveColor;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    public void Initialize(CoopDoor door, int index)
    {
        parentDoor = door;
        buttonIndex = index;

        inactiveColor = door.buttonInactiveColor;
        activeColor = door.buttonActiveColor;
        waitingColor = door.buttonWaitingColor;

        SetVisualState(ButtonState.Inactive);
    }

    void Update()
    {
        if (!playerInRange || localPlayerPV == null || parentDoor == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            PressButton();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            ReleaseButton();
        }
    }

    void PressButton()
    {
        if (isPressed) return;

        isPressed = true;
        parentDoor.OnButtonPressed(buttonIndex, localPlayerTeam, localPlayerPV.ViewID);
    }

    void ReleaseButton()
    {
        if (!isPressed) return;

        isPressed = false;
        parentDoor.OnButtonReleased(buttonIndex);
    }

    void OnTriggerEnter(Collider other)
    {
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null)
        {
            pv = other.GetComponentInParent<PhotonView>();
        }

        if (pv != null && pv.IsMine)
        {
            playerInRange = true;
            localPlayerPV = pv;

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
            {
                localPlayerTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            }

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }

            Debug.Log("CoopDoorButton: Jugador en rango del botón " + buttonIndex);
        }
    }

    void OnTriggerExit(Collider other)
    {
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null)
        {
            pv = other.GetComponentInParent<PhotonView>();
        }

        if (pv != null && pv.IsMine)
        {
            if (isPressed)
            {
                ReleaseButton();
            }

            playerInRange = false;
            localPlayerPV = null;
            localPlayerTeam = "";

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            Debug.Log("CoopDoorButton: Jugador salió del rango del botón " + buttonIndex);
        }
    }

    public void SetVisualState(ButtonState state)
    {
        if (buttonMaterial == null) return;

        switch (state)
        {
            case ButtonState.Inactive:
                buttonMaterial.color = inactiveColor;
                break;
            case ButtonState.Active:
                buttonMaterial.color = activeColor;
                break;
            case ButtonState.Waiting:
                buttonMaterial.color = waitingColor;
                break;
        }
    }

    void OnDestroy()
    {
        if (buttonMaterial != null)
        {
            Destroy(buttonMaterial);
        }
    }
}