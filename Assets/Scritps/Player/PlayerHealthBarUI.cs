using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerHealthBarUI : MonoBehaviourPun
{
    public Vector3 offsetPosition = new Vector3(0, 2.2f, 0);
    public float barWidth = 120f;
    public float barHeight = 15f;

    public Color teamAColor = new Color(0.2f, 0.5f, 1f);
    public Color teamBColor = new Color(1f, 0.25f, 0.25f);
    public Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color damageColor = new Color(1f, 1f, 1f, 0.6f);
    public Color borderColor = Color.black;

    public float healthSmoothSpeed = 8f;
    public float damageSmoothSpeed = 2f;

    private Canvas worldCanvas;
    private GameObject healthBarContainer;
    private Image damageImage;
    private Image healthImage;
    private RectTransform healthRect;
    private RectTransform damageRect;

    private Health healthComponent;
    private Camera mainCamera;
    private string playerTeam = "";

    private float displayedHealth = 1f;
    private float damageDisplayHealth = 1f;
    private float innerBarWidth;
    private bool isInitialized = false;

    private float lastHealthPercent = 1f;

    void Start()
    {
        healthComponent = GetComponent<Health>();
        innerBarWidth = barWidth - 4;

        if (photonView.Owner.CustomProperties.ContainsKey("Team"))
        {
            playerTeam = (string)photonView.Owner.CustomProperties["Team"];
        }

        if (!photonView.IsMine)
        {
            CreateHealthBarUI();

            displayedHealth = 1f;
            damageDisplayHealth = 1f;
            lastHealthPercent = 1f;

            isInitialized = true;
        }
    }

    void Update()
    {
        if (!isInitialized || worldCanvas == null || photonView.IsMine) return;

        UpdateCameraReference();

        if (mainCamera != null)
        {
            worldCanvas.transform.LookAt(
                worldCanvas.transform.position + mainCamera.transform.forward
            );
        }

        UpdateHealthBar();
    }

    void UpdateCameraReference()
    {
        if (mainCamera != null && mainCamera.enabled) return;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            PlayerCamera[] playerCams = FindObjectsOfType<PlayerCamera>();
            foreach (PlayerCamera pc in playerCams)
            {
                if (pc.mainCam != null && pc.mainCam.enabled)
                {
                    mainCamera = pc.mainCam;
                    break;
                }
            }
        }
    }

    void CreateHealthBarUI()
    {
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = offsetPosition;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one * 0.01f;

        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barWidth, barHeight);

        healthBarContainer = new GameObject("HealthBarContainer");
        healthBarContainer.transform.SetParent(canvasObj.transform, false);
        RectTransform containerRect = healthBarContainer.AddComponent<RectTransform>();
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(barWidth, barHeight);

        GameObject border = CreateUIElement("Border", healthBarContainer.transform);
        border.GetComponent<Image>().color = borderColor;
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.pivot = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(barWidth, barHeight);

        GameObject background = CreateUIElement("Background", healthBarContainer.transform);
        background.GetComponent<Image>().color = backgroundColor;
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(innerBarWidth, barHeight - 4);

        float leftEdge = -innerBarWidth / 2f;

        GameObject damage = CreateUIElement("DamageBar", healthBarContainer.transform);
        damageImage = damage.GetComponent<Image>();
        damageImage.color = damageColor;
        damageRect = damage.GetComponent<RectTransform>();
        damageRect.anchorMin = new Vector2(0.5f, 0.5f);
        damageRect.anchorMax = new Vector2(0.5f, 0.5f);
        damageRect.pivot = new Vector2(0, 0.5f);
        damageRect.anchoredPosition = new Vector2(leftEdge, 0);
        damageRect.sizeDelta = new Vector2(innerBarWidth, barHeight - 4);

        GameObject health = CreateUIElement("HealthBar", healthBarContainer.transform);
        healthImage = health.GetComponent<Image>();
        healthImage.color = GetTeamColor();
        healthRect = health.GetComponent<RectTransform>();
        healthRect.anchorMin = new Vector2(0.5f, 0.5f);
        healthRect.anchorMax = new Vector2(0.5f, 0.5f);
        healthRect.pivot = new Vector2(0, 0.5f);
        healthRect.anchoredPosition = new Vector2(leftEdge, 0);
        healthRect.sizeDelta = new Vector2(innerBarWidth, barHeight - 4);
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        obj.AddComponent<Image>();
        return obj;
    }

    void UpdateHealthBar()
    {
        if (healthComponent == null || healthRect == null || damageRect == null) return;

        float currentHealthPercent = healthComponent.GetHealthPercent();

        if (currentHealthPercent > lastHealthPercent + 0.5f)
        {
            displayedHealth = currentHealthPercent;
            damageDisplayHealth = currentHealthPercent;
        }

        lastHealthPercent = currentHealthPercent;

        displayedHealth = Mathf.Lerp(displayedHealth, currentHealthPercent, Time.deltaTime * healthSmoothSpeed);

        if (damageDisplayHealth > displayedHealth)
        {
            damageDisplayHealth = Mathf.Lerp(damageDisplayHealth, displayedHealth, Time.deltaTime * damageSmoothSpeed);
        }
        else
        {
            damageDisplayHealth = displayedHealth;
        }

        float healthWidth = innerBarWidth * Mathf.Clamp01(displayedHealth);
        float damageWidth = innerBarWidth * Mathf.Clamp01(damageDisplayHealth);

        healthRect.sizeDelta = new Vector2(healthWidth, barHeight - 4);
        damageRect.sizeDelta = new Vector2(damageWidth, barHeight - 4);
    }

    Color GetTeamColor()
    {
        if (playerTeam == "A")
            return teamAColor;
        else if (playerTeam == "B")
            return teamBColor;
        else
            return Color.white;
    }

    public void OnRespawn()
    {
        displayedHealth = 1f;
        damageDisplayHealth = 1f;
        lastHealthPercent = 1f;

        if (healthRect != null && damageRect != null)
        {
            healthRect.sizeDelta = new Vector2(innerBarWidth, barHeight - 4);
            damageRect.sizeDelta = new Vector2(innerBarWidth, barHeight - 4);
        }
    }

    void OnDestroy()
    {
        if (worldCanvas != null)
        {
            Destroy(worldCanvas.gameObject);
        }
    }
}