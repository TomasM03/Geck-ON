using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Nickname : MonoBehaviour
{
    [Header("Nickname Configuration")]
    public Canvas nicknameCanvas;
    public Text nicknameText;
    public float heightOffset = 2f;
    public Color localTextColor = Color.green;
    public Color remoteTextColor = Color.white;

    private PhotonView pv;
    private Camera playerCamera;

    void Start()
    {
        pv = GetComponent<PhotonView>();

        // Create canvas and text if they don't exist
        if (nicknameCanvas == null || nicknameText == null)
        {
            CreateNicknameUI();
        }

        // Configure the nickname
        ConfigureNickname();

        // Find the camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
    }

    void CreateNicknameUI()
    {
        // Create the canvas
        GameObject canvasGO = new GameObject("NicknameCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = Vector3.up * heightOffset;

        nicknameCanvas = canvasGO.AddComponent<Canvas>();
        nicknameCanvas.renderMode = RenderMode.WorldSpace;
        nicknameCanvas.worldCamera = Camera.main;

        // Configure canvas RectTransform
        RectTransform canvasRect = nicknameCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(3, 1);
        canvasRect.localScale = Vector3.one * 0.01f;

        // Create the text
        GameObject textGO = new GameObject("NicknameText");
        textGO.transform.SetParent(canvasGO.transform);

        nicknameText = textGO.AddComponent<Text>();
        nicknameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nicknameText.fontSize = 50;
        nicknameText.alignment = TextAnchor.MiddleCenter;

        // Configure text RectTransform
        RectTransform textRect = nicknameText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
    }

    void ConfigureNickname()
    {
        if (pv.IsMine)
        {
            // It's our player
            nicknameText.text = PhotonNetwork.LocalPlayer.NickName + " (YOU)";
            nicknameText.color = localTextColor;
        }
        else
        {
            // It's a remote player
            nicknameText.text = pv.Owner.NickName;
            nicknameText.color = remoteTextColor;
        }
    }

    void Update()
    {
        // Make nickname always face the camera
        if (playerCamera != null && nicknameCanvas != null)
        {
            Vector3 direction = nicknameCanvas.transform.position - playerCamera.transform.position;
            nicknameCanvas.transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
