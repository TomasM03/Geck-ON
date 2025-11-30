using UnityEngine;
using Photon.Pun;
using System.Collections;

public class CoopDoor : MonoBehaviourPun
{
    [Header("Door Settings")]
    [Tooltip("El objeto de la puerta (campo de fuerza) que se desactivará")]
    public GameObject doorObject;

    [Header("Button References")]
    [Tooltip("Botón izquierdo")]
    public CoopDoorButton buttonLeft;
    [Tooltip("Botón derecho")]
    public CoopDoorButton buttonRight;

    [Header("Visual Feedback")]
    [Tooltip("Color del botón cuando NO está siendo presionado")]
    public Color buttonInactiveColor = Color.red;
    [Tooltip("Color del botón cuando está siendo presionado")]
    public Color buttonActiveColor = Color.green;
    [Tooltip("Color del botón cuando está esperando al otro jugador")]
    public Color buttonWaitingColor = Color.yellow;

    [Header("Audio (Opcional)")]
    public AudioClip doorOpenSound;
    public AudioClip buttonPressSound;
    public AudioClip buttonReleaseSound;

    private AudioSource audioSource;
    private bool isDoorOpen = false;

    private string leftButtonTeam = "";
    private string rightButtonTeam = "";

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1f;

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
    public void OnButtonPressed(int buttonIndex, string playerTeam, int playerViewID)
    {
        photonView.RPC("SyncButtonPress", RpcTarget.All, buttonIndex, playerTeam, playerViewID);
    }
    public void OnButtonReleased(int buttonIndex)
    {
        photonView.RPC("SyncButtonRelease", RpcTarget.All, buttonIndex);
    }

    [PunRPC]
    void SyncButtonPress(int buttonIndex, string playerTeam, int playerViewID)
    {
        if (isDoorOpen) return;

        if (buttonIndex == 0) 
        {
            leftButtonTeam = playerTeam;
            if (buttonLeft != null)
            {
                buttonLeft.SetVisualState(ButtonState.Active);
            }
        }
        else
        {
            rightButtonTeam = playerTeam;
            if (buttonRight != null)
            {
                buttonRight.SetVisualState(ButtonState.Active);
            }
        }

     //   PlaySound(buttonPressSound);

        UpdateWaitingStates();

        CheckDoorOpen();
    }

    [PunRPC]
    void SyncButtonRelease(int buttonIndex)
    {
        if (isDoorOpen) return;

        if (buttonIndex == 0)
        {
            leftButtonTeam = "";
            if (buttonLeft != null)
            {
                buttonLeft.SetVisualState(ButtonState.Inactive);
            }
        }
        else
        {
            rightButtonTeam = "";
            if (buttonRight != null)
            {
                buttonRight.SetVisualState(ButtonState.Inactive);
            }
        }

      //  PlaySound(buttonReleaseSound);
        UpdateWaitingStates();
    }

    void UpdateWaitingStates()
    {
        bool leftActive = !string.IsNullOrEmpty(leftButtonTeam);
        bool rightActive = !string.IsNullOrEmpty(rightButtonTeam);

        if (leftActive && !rightActive && buttonLeft != null)
        {
            buttonLeft.SetVisualState(ButtonState.Waiting);
        }
        if (rightActive && !leftActive && buttonRight != null)
        {
            buttonRight.SetVisualState(ButtonState.Waiting);
        }
    }

    void CheckDoorOpen()
    {
        if (string.IsNullOrEmpty(leftButtonTeam) || string.IsNullOrEmpty(rightButtonTeam))
        {
            return;
        }
        if (leftButtonTeam != rightButtonTeam)
        {
            Debug.Log("CoopDoor: Los jugadores son de equipos diferentes. La puerta no se abre.");
            return;
        }

        OpenDoor();
    }

    void OpenDoor()
    {
        if (isDoorOpen) return;

        isDoorOpen = true;
        Debug.Log("CoopDoor: ¡Puerta abierta por el equipo " + leftButtonTeam + "!");

     //   PlaySound(doorOpenSound);

        if (doorObject != null)
        {
            doorObject.SetActive(false);
        }

        if (buttonLeft != null)
        {
            buttonLeft.SetVisualState(ButtonState.Active);
            buttonLeft.enabled = false;
        }
        if (buttonRight != null)
        {
            buttonRight.SetVisualState(ButtonState.Active);
            buttonRight.enabled = false;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
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
        leftButtonTeam = "";
        rightButtonTeam = "";

        if (doorObject != null)
        {
            doorObject.SetActive(true);
        }

        if (buttonLeft != null)
        {
            buttonLeft.enabled = true;
            buttonLeft.SetVisualState(ButtonState.Inactive);
        }
        if (buttonRight != null)
        {
            buttonRight.enabled = true;
            buttonRight.SetVisualState(ButtonState.Inactive);
        }
    }
}
public enum ButtonState
{
    Inactive,
    Active,
    Waiting
}