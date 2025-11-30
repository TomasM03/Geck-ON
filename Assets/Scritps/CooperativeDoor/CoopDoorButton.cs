using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Collider))]
public class CoopDoorButton : MonoBehaviour
{
    [Header("Visual")]
    [Tooltip("Renderer del botón para cambiar color")]
    public Renderer buttonRenderer;

    [Header("UI Prompt")]
    [Tooltip("Texto que aparece cuando el jugador está cerca")]
    public GameObject interactPrompt;
    public string promptText = "Mantener [E] para activar";

    [Header("Colors (se sobreescriben por CoopDoor)")]
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
        // Obtener renderer si no está asignado
        if (buttonRenderer == null)
        {
            buttonRenderer = GetComponent<Renderer>();
        }

        // Crear material instance para no afectar otros objetos
        if (buttonRenderer != null)
        {
            buttonMaterial = new Material(buttonRenderer.material);
            buttonRenderer.material = buttonMaterial;
            buttonMaterial.color = inactiveColor;
        }

        // Asegurar que el collider es trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Ocultar prompt al inicio
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    /// <summary>
    /// Inicializar el botón (llamado por CoopDoor)
    /// </summary>
    public void Initialize(CoopDoor door, int index)
    {
        parentDoor = door;
        buttonIndex = index;

        // Copiar colores del parent
        inactiveColor = door.buttonInactiveColor;
        activeColor = door.buttonActiveColor;
        waitingColor = door.buttonWaitingColor;

        SetVisualState(ButtonState.Inactive);
    }

    void Update()
    {
        if (!playerInRange || localPlayerPV == null || parentDoor == null) return;

        // Detectar si el jugador mantiene E
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
        // Verificar si es el jugador local
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv == null)
        {
            pv = other.GetComponentInParent<PhotonView>();
        }

        if (pv != null && pv.IsMine)
        {
            playerInRange = true;
            localPlayerPV = pv;

            // Obtener equipo del jugador
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
            {
                localPlayerTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            }

            // Mostrar prompt
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
            // Si el jugador sale del rango mientras mantiene presionado, soltar
            if (isPressed)
            {
                ReleaseButton();
            }

            playerInRange = false;
            localPlayerPV = null;
            localPlayerTeam = "";

            // Ocultar prompt
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            Debug.Log("CoopDoorButton: Jugador salió del rango del botón " + buttonIndex);
        }
    }

    /// <summary>
    /// Cambiar el estado visual del botón
    /// </summary>
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
        // Limpiar material
        if (buttonMaterial != null)
        {
            Destroy(buttonMaterial);
        }
    }
}
