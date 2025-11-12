using UnityEngine;
using LootLocker.Requests;
using System.Collections;

public class LootLockerManager : MonoBehaviour
{
    public static LootLockerManager Instance;

    private string playerID = "";
    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(InitializeLootLocker());
    }

    IEnumerator InitializeLootLocker()
    {
        bool done = false;

        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("LootLocker inicializado correctamente");
                playerID = response.player_id.ToString();
                isInitialized = true;

                if (GameManager.Instance != null)
                {
                    SetPlayerName(GameManager.Instance.GetNickname());
                }
            }
            else
            {
                Debug.LogError("Error al inicializar LootLocker: " + response.errorData.message);
            }
            done = true;
        });

        yield return new WaitUntil(() => done);
    }

    public void SetPlayerName(string playerName)
    {
        if (!isInitialized) return;

        LootLockerSDKManager.SetPlayerName(playerName, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Nombre de jugador seteado: " + playerName);
            }
            else
            {
                Debug.LogError("Error al setear nombre: " + response.errorData.message);
            }
        });
    }

    public void SubmitScore(int score, System.Action<bool> callback = null)
    {
        if (!isInitialized)
        {
            Debug.LogError("LootLocker no está inicializado");
            callback?.Invoke(false);
            return;
        }

        string leaderboardKey = "kills_leaderboard";

        LootLockerSDKManager.SubmitScore(playerID, score, leaderboardKey, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"Score enviado correctamente: {score}");
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError("Error al enviar score: " + response.errorData.message);
                callback?.Invoke(false);
            }
        });
    }

    public void GetLeaderboard(int count, System.Action<LootLockerLeaderboardMember[]> callback)
    {
        if (!isInitialized)
        {
            Debug.LogError("LootLocker no está inicializado");
            callback?.Invoke(null);
            return;
        }

        string leaderboardKey = "kills_leaderboard";

        Debug.Log($"Obteniendo leaderboard: {leaderboardKey}");

        LootLockerSDKManager.GetScoreList(leaderboardKey, count, 0, (response) =>
        {
            if (response.success)
            {
                if (response.items != null && response.items.Length > 0)
                {
                    Debug.Log($"Leaderboard obtenido: {response.items.Length} entradas");
                    callback?.Invoke(response.items);
                }
                else
                {
                    Debug.Log("Leaderboard está vacío (sin scores aún)");
                    callback?.Invoke(new LootLockerLeaderboardMember[0]);
                }
            }
            else
            {
                Debug.LogError("Error al obtener leaderboard: " + response.errorData.message);
                callback?.Invoke(null);
            }
        });
    }
}