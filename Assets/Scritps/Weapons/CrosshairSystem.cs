using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairSystem : MonoBehaviour
{
    [Header("Crosshair Images - Asignar por Inspector")]
    [Tooltip("Imagen del crosshair cuando no apunta a nadie")]
    public Sprite crosshairNeutral;
    [Tooltip("Imagen del crosshair cuando apunta a un enemigo")]
    public Sprite crosshairEnemy;
    [Tooltip("Imagen del crosshair cuando apunta a un aliado")]
    public Sprite crosshairAlly;
    [Tooltip("Imagen del hitmarker al impactar")]
    public Sprite hitmarkerSprite;

    [Header("UI References")]
    [Tooltip("Imagen UI del crosshair (crear un Image en el Canvas)")]
    public Image crosshairImage;
    [Tooltip("Imagen UI del hitmarker (crear otro Image en el Canvas, hijo del crosshair)")]
    public Image hitmarkerImage;

    [Header("Colors")]
    public Color neutralColor = Color.white;
    public Color enemyColor = Color.red;
    public Color allyColor = new Color(0.3f, 0.6f, 1f); // Azul
    public Color hitmarkerColor = Color.white;

    [Header("Detection Settings")]
    public float detectionRange = 100f;
    public LayerMask detectionLayers = -1;

    [Header("Hitmarker Settings")]
    public float hitmarkerDuration = 0.15f;
    public float hitmarkerScale = 1.5f;
    public Vector2 hitmarkerSize = new Vector2(50f, 50f);

    [Header("Audio")]
    public AudioClip hitmarkerSound;
    [Range(0f, 1f)]
    public float hitmarkerVolume = 0.5f;

    [Header("Size Settings")]
    public Vector2 crosshairSize = new Vector2(32f, 32f);

    [Header("Debug")]
    public bool showDebugRay = true;

    private PhotonView localPlayerPV;
    private string myTeam = "";
    private AudioSource audioSource;
    private Coroutine hitmarkerCoroutine;

    // Referencia a la cámara del jugador
    private Camera playerCamera;

    // Singleton para acceso fácil desde Weapon
    public static CrosshairSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        SetupAudioSource();
        SetupUI();
        StartCoroutine(FindLocalPlayer());
    }

    void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    void SetupUI()
    {
        if (crosshairImage != null)
        {
            crosshairImage.rectTransform.sizeDelta = crosshairSize;
            if (crosshairNeutral != null)
            {
                crosshairImage.sprite = crosshairNeutral;
            }
            crosshairImage.color = neutralColor;
        }

        if (hitmarkerImage != null)
        {
            hitmarkerImage.rectTransform.sizeDelta = hitmarkerSize;
            if (hitmarkerSprite != null)
            {
                hitmarkerImage.sprite = hitmarkerSprite;
            }
            hitmarkerImage.color = hitmarkerColor;
            hitmarkerImage.enabled = false;
        }
    }

    IEnumerator FindLocalPlayer()
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
                        myTeam = (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
                    }

                    // Buscar la cámara del jugador
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
        if (localPlayerPV == null) return;

        // Buscar cámara si no la tenemos
        if (playerCamera == null)
        {
            FindPlayerCamera();
        }

        UpdateCrosshair();

        // Debug ray
        if (showDebugRay && playerCamera != null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Debug.DrawRay(ray.origin, ray.direction * detectionRange, Color.yellow);
        }
    }

    void FindPlayerCamera()
    {
        if (localPlayerPV != null)
        {
            PlayerCamera playerCam = localPlayerPV.GetComponentInChildren<PlayerCamera>();
            if (playerCam != null && playerCam.mainCam != null)
            {
                playerCamera = playerCam.mainCam;
            }
        }
    }

    void UpdateCrosshair()
    {
        // Si no hay cámara, no podemos detectar
        if (playerCamera == null)
        {
            SetCrosshairState(CrosshairState.Neutral);
            return;
        }

        // RAYCAST DESDE EL CENTRO DE LA CÁMARA - Igual que Weapon.cs
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, detectionRange, detectionLayers))
        {
            // Buscar Health igual que en Weapon.cs
            Health targetHealth = hit.collider.GetComponent<Health>();
            if (targetHealth == null)
            {
                targetHealth = hit.collider.GetComponentInParent<Health>();
            }

            if (targetHealth != null)
            {
                PhotonView targetPV = targetHealth.GetComponent<PhotonView>();

                if (targetPV != null && targetPV.ViewID != localPlayerPV.ViewID)
                {
                    // Es otro jugador, verificar equipo
                    string targetTeam = "";
                    if (targetPV.Owner != null && targetPV.Owner.CustomProperties.ContainsKey("Team"))
                    {
                        targetTeam = (string)targetPV.Owner.CustomProperties["Team"];
                    }

                    if (!string.IsNullOrEmpty(targetTeam) && !string.IsNullOrEmpty(myTeam))
                    {
                        if (targetTeam == myTeam)
                        {
                            SetCrosshairState(CrosshairState.Ally);
                        }
                        else
                        {
                            SetCrosshairState(CrosshairState.Enemy);
                        }
                    }
                    else
                    {
                        SetCrosshairState(CrosshairState.Neutral);
                    }
                    return;
                }
            }
        }

        // Si no pegó a ningún jugador
        SetCrosshairState(CrosshairState.Neutral);
    }

    void SetCrosshairState(CrosshairState state)
    {
        if (crosshairImage == null) return;

        switch (state)
        {
            case CrosshairState.Neutral:
                if (crosshairNeutral != null) crosshairImage.sprite = crosshairNeutral;
                crosshairImage.color = neutralColor;
                break;

            case CrosshairState.Enemy:
                if (crosshairEnemy != null) crosshairImage.sprite = crosshairEnemy;
                crosshairImage.color = enemyColor;
                break;

            case CrosshairState.Ally:
                if (crosshairAlly != null) crosshairImage.sprite = crosshairAlly;
                crosshairImage.color = allyColor;
                break;
        }
    }

    /// <summary>
    /// Llamar este método cuando se impacta a un enemigo
    /// </summary>
    public void ShowHitmarker()
    {
        if (hitmarkerCoroutine != null)
        {
            StopCoroutine(hitmarkerCoroutine);
        }
        hitmarkerCoroutine = StartCoroutine(HitmarkerAnimation());
    }

    IEnumerator HitmarkerAnimation()
    {
        if (hitmarkerImage == null) yield break;

        hitmarkerImage.enabled = true;
        hitmarkerImage.color = hitmarkerColor;

        if (hitmarkerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitmarkerSound, hitmarkerVolume);
        }

        float elapsed = 0f;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * hitmarkerScale;

        while (elapsed < hitmarkerDuration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (hitmarkerDuration * 0.3f);
            hitmarkerImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < hitmarkerDuration * 0.7f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (hitmarkerDuration * 0.7f);

            Color c = hitmarkerColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            hitmarkerImage.color = c;

            hitmarkerImage.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);

            yield return null;
        }

        hitmarkerImage.enabled = false;
        hitmarkerImage.transform.localScale = originalScale;
    }

    public void UpdateMyTeam(string newTeam)
    {
        myTeam = newTeam;
    }

    private enum CrosshairState
    {
        Neutral,
        Enemy,
        Ally
    }
}
