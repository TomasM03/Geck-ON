using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Photon.Pun;

public class KillFeed : MonoBehaviourPun
{
    [Header("UI References")]
    public Transform feedContainer;
    public GameObject killFeedItemPrefab;

    [Header("Settings")]
    public int maxFeedItems = 5;
    public float itemLifetime = 5f;
    public Color teamAColor = new Color(0.3f, 0.6f, 1f);
    public Color teamBColor = new Color(1f, 0.3f, 0.3f);

    private List<KillFeedItem> activeFeedItems = new List<KillFeedItem>();

    void Start()
    {
        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.onKillRegistered += OnKillRegistered;
        }
    }

    void OnKillRegistered(string killerTeam, int totalKills)
    {
        ShowKillNotification(killerTeam, totalKills);
    }

    [PunRPC]
    void ShowKillNotification(string killerTeam, int totalKills)
    {
        if (killFeedItemPrefab == null || feedContainer == null) return;

        GameObject itemObj = Instantiate(killFeedItemPrefab, feedContainer);
        TMP_Text itemText = itemObj.GetComponent<TMP_Text>();

        if (itemText != null)
        {
            itemText.text = killerTeam + " scored! [" + totalKills + "]";
            itemText.color = killerTeam == "Team A" ? teamAColor : teamBColor;
        }

        KillFeedItem feedItem = new KillFeedItem
        {
            gameObject = itemObj,
            lifetime = itemLifetime
        };

        activeFeedItems.Add(feedItem);

        while (activeFeedItems.Count > maxFeedItems)
        {
            RemoveFeedItem(0);
        }
    }

    void Update()
    {
        for (int i = activeFeedItems.Count - 1; i >= 0; i--)
        {
            activeFeedItems[i].lifetime -= Time.deltaTime;

            if (activeFeedItems[i].lifetime <= 0)
            {
                RemoveFeedItem(i);
            }
        }
    }

    void RemoveFeedItem(int index)
    {
        if (index >= 0 && index < activeFeedItems.Count)
        {
            if (activeFeedItems[index].gameObject != null)
            {
                Destroy(activeFeedItems[index].gameObject);
            }
            activeFeedItems.RemoveAt(index);
        }
    }

    void OnDestroy()
    {
        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.onKillRegistered -= OnKillRegistered;
        }
    }

    private class KillFeedItem
    {
        public GameObject gameObject;
        public float lifetime;
    }
}