using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairSystem : MonoBehaviour
{
    public Sprite crosshairNeutral;
    public Sprite crosshairEnemy;
    public Sprite crosshairAlly;
    public Sprite hitmarkerSprite;

    public Image crosshairImage;
    public Image hitmarkerImage;

    public Color neutralColor = Color.white;
    public Color enemyColor = Color.red;
    public Color allyColor = new Color(0.3f, 0.6f, 1f);
    public Color hitmarkerColor = Color.white;

    public float detectionRange = 100f;
    public LayerMask detectionLayers = -1;

    public float hitmarkerDuration = 0.15f;
    public float hitmarkerScale = 1.5f;
    public Vector2 hitmarkerSize = new Vector2(50f, 50f);
    public Vector2 crosshairSize = new Vector2(32f, 32f);

    private PhotonView localPlayerPV;
    private string myTeam = "";
    private Coroutine hitmarkerCoroutine;
    private Camera playerCamera;

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
        SetupUI();
        StartCoroutine(FindLocalPlayer());
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

        if (playerCamera == null)
        {
            FindPlayerCamera();
        }

        UpdateCrosshair();
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
        if (playerCamera == null)
        {
            SetCrosshairState(CrosshairState.Neutral);
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, detectionRange, detectionLayers))
        {
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

    private enum CrosshairState
    {
        Neutral,
        Enemy,
        Ally
    }
}