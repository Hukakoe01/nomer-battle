using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MultiplayerManager : MonoBehaviour
{
    [Header("UI")]
    public Button playButton;

    private string baseUrl;
    private int playerId = -1;
    private const string sceneToLoad = "MultiplayerGameScene";

    void Start()
    {
        Debug.Log("[MULTIPLAYER MANAGER] Запуск менеджера");
        baseUrl = "http://" + PlayerPrefs.GetString("ServerIP", "localhost") + ":3000";

        playButton.interactable = false;
        playButton.onClick.AddListener(OnPlayClicked);

        if (PlayerSession.playerType == "host")
        {
            StartCoroutine(WaitForServerAndJoin());
        }
        else
        {
            // Если это гость, он уже зарегистрировался раньше
            int storedId = PlayerPrefs.GetInt("PlayerID", -1);

            if (storedId >= 0)
            {
                Debug.Log("Гость уже зарегистрирован. ID: " + storedId);
                playerId = storedId;
                playButton.interactable = true;
            }
            else
            {
                Debug.LogWarning("Гость не зарегистрирован. Возможно, ошибка при переходе.");
            }
        }
    }

    IEnumerator WaitForServerAndJoin()
    {
        string testUrl = baseUrl + "/state";
        const int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            UnityWebRequest req = UnityWebRequest.Get(testUrl);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Сервер доступен. Отправляем /join...");
                yield return StartCoroutine(RegisterPlayer());
                yield break;
            }

            Debug.Log($"Ожидание сервера... попытка {attempt + 1}");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.LogError("Сервер не отвечает. Проверь IP и запущен ли он.");
    }

    IEnumerator RegisterPlayer()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Player"); // <-- изменено

        WWWForm form = new WWWForm();
        form.AddField("playerName", playerName);

        Debug.Log("[JOIN] Registering with name: " + playerName);

        UnityWebRequest req = UnityWebRequest.Post($"{baseUrl}/join", form);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = req.downloadHandler.text;
            PlayerIdResponse playerData = JsonUtility.FromJson<PlayerIdResponse>(json);

            playerId = playerData.playerId;
            PlayerPrefs.SetInt("PlayerID", playerId);
            playButton.interactable = true;

            Debug.Log("Successfully joined. Player ID: " + playerId);
        }
        else
        {
            Debug.LogError("JOIN request failed: " + req.error);
            Debug.Log("Response body: " + req.downloadHandler.text);
        }
    }

    void OnPlayClicked()
    {
        if (playerId >= 0)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Ошибка: попытка начать игру без Player ID.");
        }
    }

    [System.Serializable]
    public class PlayerIdResponse
    {
        public int playerId;
    }
}