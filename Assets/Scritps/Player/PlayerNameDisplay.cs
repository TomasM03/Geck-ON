using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerNameDisplay : MonoBehaviour
{
    public GameObject prefabName;
    public Vector3 offsetPosicion = new Vector3(0, 2f, 0);
    public bool cameraView = true;

    public Color localPlayerColor = Color.green;
    public Color otherPlayerColor = Color.white;

    private PhotonView pv;
    private GameObject nameUI;
    private TextMeshPro nameText;
    private Camera mainCam;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        mainCam = Camera.main;
        CreateName();
    }

    void CreateName()
    {
        if (prefabName == null)
        {
            BasicName();
        }
        else
        {
            nameUI = Instantiate(prefabName, transform);
            nameText = nameUI.GetComponent<TextMeshPro>();
        }

        if (nameText != null)
        {
            NameSet();
        }
    }

    void BasicName()
    {
        nameUI = new GameObject("PlayerName");
        nameUI.transform.SetParent(transform);
        nameUI.transform.localPosition = offsetPosicion;
            
        nameText = nameUI.AddComponent<TextMeshPro>();
        nameText.text = "Name";
        nameText.fontSize = 3;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.sortingOrder = 10;
    }

    void NameSet()
    {
        if (pv != null && nameText != null)
        {
            nameText.text = pv.Owner.NickName;

            if (pv.IsMine)
            {
                nameText.color = localPlayerColor;
                nameText.text = nameText.text + " (YOU)";
            }   
            else
            {
                nameText.color = otherPlayerColor;
            }

            nameUI.transform.localPosition = offsetPosicion;
        }
    }

}