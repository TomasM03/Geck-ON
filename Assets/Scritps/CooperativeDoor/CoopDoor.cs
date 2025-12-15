using UnityEngine;
using Photon.Pun;

public class CoopDoor : MonoBehaviourPun
{
    [Header("Referencias")]
    public GameObject doorObject;
    public CoopDoorButton buttonLeft;
    public CoopDoorButton buttonRight;

    [Header("Colores de Botones")]
    public Color buttonInactiveColor = Color.red;
    public Color buttonActiveColor = Color.green;
    public Color buttonReadyColor = Color.yellow;

    [Header("Configuración de Sincronización")]
    public float syncTimeWindow = 1.5f;

    private bool isDoorOpen = false;
    private bool leftButtonPressed = false;
    private bool rightButtonPressed = false;
    private float leftButtonTime = -999f;
    private float rightButtonTime = -999f;
    private string leftButtonTeam = "";
    private string rightButtonTeam = "";

    void Start()
    {
        if (buttonLeft != null)
        {
            buttonLeft.Initialize(this, 0);
        }
        if (buttonRight != null)
        {
            buttonRight.Initialize(this, 1);
        }

        if (doorObject != null)
        {
            doorObject.SetActive(true);
        }
    }

    public void OnButtonActivated(int buttonIndex, string playerTeam, int playerViewID)
    {
        photonView.RPC("SyncButtonActivation", RpcTarget.All, buttonIndex, playerTeam, playerViewID, (float)PhotonNetwork.Time);
    }

    [PunRPC]
    void SyncButtonActivation(int buttonIndex, string playerTeam, int playerViewID, float activationTime)
    {
        if (isDoorOpen) return;

        if (buttonIndex == 0)
        {
            leftButtonPressed = true;
            leftButtonTime = activationTime;
            leftButtonTeam = playerTeam;

            if (buttonLeft != null)
            {
                buttonLeft.SetVisualState(ButtonState.Active);
            }

            Debug.Log($"CoopDoor: Botón izquierdo activado por equipo {playerTeam} a tiempo {activationTime}");
        }
        else if (buttonIndex == 1)
        {
            rightButtonPressed = true;
            rightButtonTime = activationTime;
            rightButtonTeam = playerTeam;

            if (buttonRight != null)
            {
                buttonRight.SetVisualState(ButtonState.Active);
            }

            Debug.Log($"CoopDoor: Botón derecho activado por equipo {playerTeam} a tiempo {activationTime}");
        }

        UpdateReadyStates();
        CheckDoorOpen();
    }

    void UpdateReadyStates()
    {
        if (isDoorOpen) return;

        if (leftButtonPressed && !rightButtonPressed && buttonLeft != null)
        {
            buttonLeft.SetVisualState(ButtonState.Ready);
        }

        if (rightButtonPressed && !leftButtonPressed && buttonRight != null)
        {
            buttonRight.SetVisualState(ButtonState.Ready);
        }
    }

    void CheckDoorOpen()
    {
        if (!leftButtonPressed || !rightButtonPressed)
        {
            return;
        }

        if (leftButtonTeam != rightButtonTeam)
        {
            Debug.Log("CoopDoor: Los jugadores son de equipos diferentes. La puerta no se abre.");
            return;
        }

        float timeDifference = Mathf.Abs(leftButtonTime - rightButtonTime);
        Debug.Log($"CoopDoor: Diferencia de tiempo entre botones: {timeDifference}s (máximo: {syncTimeWindow}s)");

        if (timeDifference <= syncTimeWindow)
        {
            OpenDoor();
        }
        else
        {
            Debug.Log("CoopDoor: Los botones no fueron presionados al mismo tiempo. Reseteando...");
            ResetButtons();
        }
    }

    void OpenDoor()
    {
        if (isDoorOpen) return;

        isDoorOpen = true;
        Debug.Log($"CoopDoor: ¡Puerta abierta por el equipo {leftButtonTeam}!");

        if (doorObject != null)
        {
            doorObject.SetActive(false);
        }

        if (buttonLeft != null)
        {
            buttonLeft.SetVisualState(ButtonState.Active);
        }
        if (buttonRight != null)
        {
            buttonRight.SetVisualState(ButtonState.Active);
        }
    }

    void ResetButtons()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncResetButtons", RpcTarget.All);
        }
    }

    [PunRPC]
    void SyncResetButtons()
    {
        leftButtonPressed = false;
        rightButtonPressed = false;
        leftButtonTime = -999f;
        rightButtonTime = -999f;
        leftButtonTeam = "";
        rightButtonTeam = "";

        if (buttonLeft != null)
        {
            buttonLeft.ResetButton();
        }
        if (buttonRight != null)
        {
            buttonRight.ResetButton();
        }
    }

    public void ResetDoor()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncResetDoor", RpcTarget.All);
        }
    }

    [PunRPC]
    void SyncResetDoor()
    {
        isDoorOpen = false;
        leftButtonPressed = false;
        rightButtonPressed = false;
        leftButtonTime = -999f;
        rightButtonTime = -999f;
        leftButtonTeam = "";
        rightButtonTeam = "";

        if (doorObject != null)
        {
            doorObject.SetActive(true);
        }

        if (buttonLeft != null)
        {
            buttonLeft.ResetButton();
        }
        if (buttonRight != null)
        {
            buttonRight.ResetButton();
        }

        Debug.Log("CoopDoor: Puerta reseteada completamente");
    }

    public bool IsDoorOpen()
    {
        return isDoorOpen;
    }
}