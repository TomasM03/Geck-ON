using UnityEngine;
using Photon.Pun;

public class CoopDoorButton : MonoBehaviourPun
{
    [Header("Visual")]
    public Renderer buttonRenderer;
    public Color inactiveColor = Color.red;
    public Color activeColor = Color.green;

    [Header("Configuration")]
    public int doorID = 0;
    public string requiredTeam = "A";

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
            SetColor(inactiveColor);
        }
    }

    public void TryActivate(string playerTeam, int playerViewID)
    {
        if (isActivated)
        {
            return;
        }

        if (playerTeam != requiredTeam)
        {
            return;
        }

        photonView.RPC("ActivateButton", RpcTarget.All, playerViewID);
    }

    [PunRPC]
    void ActivateButton(int playerViewID)
    {
        isActivated = true;
        SetColor(activeColor);

        CoopDoor doorSystem = FindObjectOfType<CoopDoor>();
        if (doorSystem != null)
        {
            doorSystem.OnButtonActivated(doorID, requiredTeam);
        }
        else
        {
            Debug.LogError("CoopDoorSystem not");
        }
    }

    void SetColor(Color color)
    {
        if (buttonMaterial != null)
        {
            buttonMaterial.color = color;
        }
    }

    public void ResetButton()
    {
        isActivated = false;
        SetColor(inactiveColor);
    }

    public bool IsActivated()
    {
        return isActivated;
    }

    public int GetDoorID()
    {
        return doorID;
    }

    public string GetRequiredTeam()
    {
        return requiredTeam;
    }

    void OnDestroy()
    {
        if (buttonMaterial != null)
        {
            Destroy(buttonMaterial);
        }
    }
}