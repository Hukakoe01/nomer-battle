using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerGameLogic : MonoBehaviour
{
    public TMP_Text chatText;
    public TMP_Text healthText;
    public TMP_Text statusText;
    public Button shootButton;
    public TMP_InputField chatInput;

    private int playerId;
    private string baseUrl;

    void Start()
    {
        baseUrl = "http://" + PlayerPrefs.GetString("ServerIP", "localhost") + ":3000";
        playerId = PlayerPrefs.GetInt("PlayerID", -1);

        if (playerId == -1)
        {
            statusText.text = "Ошибка: PlayerID не найден.";
            shootButton.interactable = false;
            return;
        }

        shootButton.onClick.AddListener(OnShootClicked);
        if (chatInput != null)
        {
            chatInput.onSubmit.AddListener(OnChatSubmit);
        }

        shootButton.interactable = false;
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        HashSet<string> processedMessages = new HashSet<string>();

        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/state");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                GameState state = JsonUtility.FromJson<GameState>(json);

                if (state.players == null || state.players.Length < 2)
                {
                    statusText.text = "Ожидание второго игрока...";
                    shootButton.interactable = false;
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                if (state.chat != null)
                {
                    foreach (string message in state.chat)
                    {
                        if (!processedMessages.Contains(message))
                        {
                            chatText.text += "\n" + message;
                            processedMessages.Add(message);
                        }
                    }
                }

                if (playerId >= 0 && playerId < state.players.Length)
                {
                    int myHP = state.players[playerId].hp;
                    int enemyHP = state.players[1 - playerId].hp;
                    healthText.text = $"Ваше HP: {myHP} | Враг: {enemyHP}";

                    if (myHP <= 0 || enemyHP <= 0)
                    {
                        string result = (myHP <= 0) ? "Вы проиграли!" : "Вы победили!";
                        statusText.text = result;
                        shootButton.interactable = false;
                        yield break;
                    }
                }

                if (state.currentTurn >= 0)
                {
                    bool isMyTurn = (state.currentTurn == playerId);
                    shootButton.interactable = isMyTurn;

                    if (isMyTurn)
                    {
                        statusText.text = "Ваш ход!";
                    }
                    else
                    {
                        statusText.text = "Ход противника...";
                    }
                }
            }
            else
            {
                Debug.LogError($"Ошибка соединения: {request.error}");
                statusText.text = "Ошибка соединения с сервером";
                shootButton.interactable = false;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void OnShootClicked()
    {
        shootButton.interactable = false;
        int damage = CalculateDamage();
        StartCoroutine(SendMove(damage));
    }

    int CalculateDamage()
    {
        int totalDamage = 0;
        foreach (GunInstance gun in GameManager.Instance.gunArray)
        {
            int shotDamage = gun.damage;
            if (gun.direction == 1)
            {
                for (int tx = gun.position.x + 1; tx < GameManager.Instance.gridSize; tx++)
                {
                    Vector2Int pos = new Vector2Int(tx, gun.position.y);
                    if (GameManager.Instance.cellEffects.TryGetValue(pos, out string effect))
                    {
                        if (effect == "power")
                        {
                            shotDamage += 2;
                        }
                    }
                }
            }
            totalDamage += shotDamage;
        }
        return totalDamage;
    }

    void OnChatSubmit(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            StartCoroutine(SendChatMessage(message));
            chatInput.text = "";
            chatInput.ActivateInputField();
        }
    }

    IEnumerator SendMove(int damage)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerId", playerId);
        form.AddField("damage", damage);

        UnityWebRequest request = UnityWebRequest.Post($"{baseUrl}/move", form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Ошибка отправки хода: " + request.error);
            shootButton.interactable = true;
        }
        else
        {
            string responseText = request.downloadHandler.text;
            if (!responseText.Contains("success"))
            {
                Debug.LogWarning("Ошибка от сервера: " + responseText);
                shootButton.interactable = true;
            }
        }
    }

    IEnumerator SendChatMessage(string message)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerId", playerId);
        form.AddField("message", message);

        UnityWebRequest request = UnityWebRequest.Post($"{baseUrl}/chat", form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Ошибка отправки сообщения: " + request.error);
        }
    }

    void OnDestroy()
    {
        if (shootButton != null)
            shootButton.onClick.RemoveListener(OnShootClicked);

        if (chatInput != null)
            chatInput.onSubmit.RemoveListener(OnChatSubmit);
    }
}

[System.Serializable]
public class GameState
{
    public Player[] players;
    public int?[] actions;
    public string[] chat;
    public Dictionary<string, object> board;
    public string hostIP;
    public int currentTurn;
}

[System.Serializable]
public class Player
{
    public int id;
    public string name;
    public int hp;
}