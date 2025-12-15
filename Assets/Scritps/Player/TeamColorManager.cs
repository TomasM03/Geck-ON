using UnityEngine;
using Photon.Pun;

public class TeamColorManager : MonoBehaviourPun
{
    public Color teamAColor = Color.blue;
    public Color teamBColor = Color.red;

    public Renderer playerRenderer;
    public int materialIndex = 0;

    public bool addLocalPlayerGlow = true;
    public float glowIntensity = 0.2f;

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

            if (playerTeam == "A")
            {
                playerMaterial.color = teamAColor;
            }
            else if (playerTeam == "B")
            {
                playerMaterial.color = teamBColor;
            }

            if (photonView.IsMine && addLocalPlayerGlow)
            {
                Color currentColor = playerMaterial.color;
                playerMaterial.color = new Color(
                    Mathf.Min(1f, currentColor.r + glowIntensity),
                    Mathf.Min(1f, currentColor.g + glowIntensity),
                    Mathf.Min(1f, currentColor.b + glowIntensity),
                    currentColor.a
                );

                if (playerMaterial.HasProperty("_EmissionColor"))
                {
                    playerMaterial.EnableKeyword("_EMISSION");
                    playerMaterial.SetColor("_EmissionColor", currentColor * 0.3f);
                }
            }

            materials[materialIndex] = playerMaterial;
            playerRenderer.materials = materials;
        }
    }

    void OnDestroy()
    {
        if (playerMaterial != null)
        {
            Destroy(playerMaterial);
        }
    }
}
