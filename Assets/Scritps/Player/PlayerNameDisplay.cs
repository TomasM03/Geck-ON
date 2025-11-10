using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerNameDisplay : MonoBehaviourPun
{
    public GameObject prefabName;
    public Vector3 offsetPosicion = new Vector3(0, 2f, 0);

    public Color localPlayerColor = Color.green;
    public Color teamAColor = new Color(0.3f, 0.6f, 1f);
    public Color teamBColor = new Color(1f, 0.3f, 0.3f);

    private GameObject nameUI;
    private TextMeshPro nameText;

    void Start()
    {
        Invoke("CreateNameDelayed", 0.5f);
    }

    void CreateNameDelayed()
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
        if (photonView != null && nameText != null)
        {
            nameText.text = photonView.Owner.NickName;

            if (photonView.IsMine)
            {
                nameText.color = localPlayerColor;
                nameText.text = nameText.text + " (YOU)";
            }
            else
            {
                string team = "";
                if (photonView.Owner.CustomProperties.ContainsKey("Team"))
                {
                    team = (string)photonView.Owner.CustomProperties["Team"];
                }

                if (team == "A")
                    nameText.color = teamAColor;
                else if (team == "B")
                    nameText.color = teamBColor;
                else
                    nameText.color = Color.white;
            }

            nameUI.transform.localPosition = offsetPosicion;
        }
    }
}