using UnityEngine;
using Photon.Pun;

public class TeamColorManager : MonoBehaviourPun
{
    [Header("Team Colors")]
    public Color teamAColor = Color.blue;
    public Color teamBColor = Color.red;
    public Color localPlayerOutline = Color.yellow;

    [Header("Renderer Settings")]
    public Renderer playerRenderer;
    public bool useOutlineForLocalPlayer = true;

    [Header("Material Settings")]
    public int materialIndex = 0;

    private Material playerMaterial;

    void Start()
    {
        if (playerRenderer == null)
        {
            playerRenderer = GetComponent<Renderer>();
        }

        if (playerRenderer == null)
        {
            playerRenderer = GetComponentInChildren<Renderer>();
        }

        if (playerRenderer != null)
        {
            ApplyTeamColor();
        }
    }

    void ApplyTeamColor()
    {
        string playerTeam = "";

        if (photonView.Owner.CustomProperties.ContainsKey("Team"))
        {
            playerTeam = (string)photonView.Owner.CustomProperties["Team"];
        }

        Material[] materials = playerRenderer.materials;

        if (materialIndex < materials.Length)
        {
            playerMaterial = new Material(materials[materialIndex]);

            if (photonView.IsMine && useOutlineForLocalPlayer)
            {
                playerMaterial.color = localPlayerOutline;
            }
            else
            {
                if (playerTeam == "A")
                {
                    playerMaterial.color = teamAColor;
                }
                else if (playerTeam == "B")
                {
                    playerMaterial.color = teamBColor;
                }
            }

            materials[materialIndex] = playerMaterial;
            playerRenderer.materials = materials;
        }
    }

    public void UpdateTeamColor(string newTeam)
    {
        if (playerMaterial != null)
        {
            if (newTeam == "A")
            {
                playerMaterial.color = teamAColor;
            }
            else if (newTeam == "B")
            {
                playerMaterial.color = teamBColor;
            }
        }
    }
}