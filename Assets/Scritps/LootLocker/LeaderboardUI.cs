using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LootLocker.Requests;

public class LeaderboardUI : MonoBehaviour
{
    public GameObject leaderboardPanel;
    public Transform leaderboardContainer;
    public GameObject leaderboardEntryPrefab;
    public Button closeButton;
    public Button refreshButton;
    public int maxEntries = 10;

    void Start()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseLeaderboard);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshLeaderboard);
        }
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
        }

        RefreshLeaderboard();
    }

    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    public void RefreshLeaderboard()
    {
        if (LootLockerManager.Instance == null)
        {
            Debug.LogError("LootLockerManager no encontrado");
            return;
        }

        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        LootLockerManager.Instance.GetLeaderboard(maxEntries, OnLeaderboardLoaded);
    }

    void OnLeaderboardLoaded(LootLockerLeaderboardMember[] members)
    {
        if (members == null || members.Length == 0)
        {
            Debug.Log("No hay entradas en el leaderboard");
            return;
        }

        for (int i = 0; i < members.Length; i++)
        {
            CreateLeaderboardEntry(members[i], i + 1);
        }
    }

    void CreateLeaderboardEntry(LootLockerLeaderboardMember member, int rank)
    {
        if (leaderboardEntryPrefab == null || leaderboardContainer == null) return;

        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);

        TMP_Text[] texts = entryObj.GetComponentsInChildren<TMP_Text>();

        if (texts.Length >= 3)
        {
            texts[0].text = "#" + rank;
            texts[1].text = member.player.name;
            texts[2].text = member.score.ToString() + " kills";
        }

        if (rank == 1)
        {
            texts[0].color = Color.yellow;
        }
        else if (rank == 2)
        {
            texts[0].color = Color.green;
        }
        else if (rank == 3)
        {
            texts[0].color = Color.red;
        }
    }
}