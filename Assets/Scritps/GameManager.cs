using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string nickname = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInfo();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetNickname(string nuevoNickname)
    {
        nickname = nuevoNickname;
        SaveInfos();
        Debug.Log("Nickname guardado: " + nickname);
    }

    public string GetNickname()
    {
        if (string.IsNullOrEmpty(nickname))
        {
            nickname = "Jugador_" + Random.Range(1000, 9999);
            SaveInfos();
        }
        return nickname;
    }

    void SaveInfos()
    {
        PlayerPrefs.SetString("PlayerNickname", nickname);
        PlayerPrefs.Save();
    }

    void LoadInfo()
    {
        nickname = PlayerPrefs.GetString("PlayerNickname", "");
    }
}