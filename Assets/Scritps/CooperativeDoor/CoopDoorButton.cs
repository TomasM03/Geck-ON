using UnityEngine;
using Photon.Pun;

public class CoopDoorButton : MonoBehaviour
{
    [Header("Visual")]
    public Renderer buttonRenderer;
    public Color inactiveColor = Color.red;
    public Color activeColor = Color.green;
    public Color readyColor = Color.yellow;

    [Header("Detección")]
    public float maxInteractionDistance = 3f;
    public LayerMask interactionLayer;

    private CoopDoor parentDoor;
    private int buttonIndex;
    private Material buttonMaterial;
    private bool isActivated = false;

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

        if (interactionLayer == 0)
        {
            interactionLayer = ~0;
        }
    }

    public void Initialize(CoopDoor door, int index)
    {
        parentDoor = door;
        buttonIndex = index;

        inactiveColor = door.buttonInactiveColor;
        activeColor = door.buttonActiveColor;
        readyColor = door.buttonReadyColor;

        SetVisualState(ButtonState.Inactive);
    }

    public bool CanInteract(Vector3 playerPosition, Vector3 playerForward)
    {
        if (isActivated || parentDoor == null || parentDoor.IsDoorOpen())
            return false;

        Vector3 toButton = transform.position - playerPosition;
        float distance = toButton.magnitude;

        if (distance > maxInteractionDistance)
            return false;

        float angle = Vector3.Angle(playerForward, toButton.normalized);
        return angle < 45f;
    }

    public void ActivateButton(string playerTeam, int playerViewID)
    {
        if (isActivated || parentDoor == null)
            return;

        isActivated = true;
        parentDoor.OnButtonActivated(buttonIndex, playerTeam, playerViewID);
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
            case ButtonState.Ready:
                buttonMaterial.color = readyColor;
                break;
        }
    }

    public void ResetButton()
    {
        isActivated = false;
        SetVisualState(ButtonState.Inactive);
    }

    void OnDestroy()
    {
        if (buttonMaterial != null)
        {
            Destroy(buttonMaterial);
        }
    }

    public int GetButtonIndex()
    {
        return buttonIndex;
    }
}

public enum ButtonState
{
    Inactive,
    Active,
    Ready
}