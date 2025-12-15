using UnityEngine;
using Photon.Pun;

public class DamageVFX : MonoBehaviourPun
{
    public GameObject bloodParticlePrefab;
    public Vector3 spawnOffset = new Vector3(0, 1f, 0);
    public float particleLifetime = 2f;

    public Color teamAColor = new Color(0.2f, 0.5f, 1f);
    public Color teamBColor = new Color(1f, 0.2f, 0.2f);

    private string playerTeam = "";

    void Start()
    {
        if (photonView.Owner.CustomProperties.ContainsKey("Team"))
        {
            playerTeam = (string)photonView.Owner.CustomProperties["Team"];
        }
    }

    public void PlayDamageEffect()
    {
        photonView.RPC("SpawnBloodParticlesRPC", RpcTarget.All);
    }

    [PunRPC]
    void SpawnBloodParticlesRPC()
    {
        if (bloodParticlePrefab == null) return;

        Vector3 spawnPos = transform.position + spawnOffset;
        GameObject particles = Instantiate(bloodParticlePrefab, spawnPos, Quaternion.identity);

        Color teamColor = GetTeamColor();

        ParticleSystem ps = particles.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = teamColor;
        }

        ParticleSystem[] childPS = particles.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem child in childPS)
        {
            var main = child.main;
            main.startColor = teamColor;
        }

        Destroy(particles, particleLifetime);
    }

    Color GetTeamColor()
    {
        if (playerTeam == "A")
            return teamAColor;
        else if (playerTeam == "B")
            return teamBColor;
        else
            return Color.red;
    }
}