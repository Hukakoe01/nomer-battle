using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class MultiplayerGame : MonoBehaviour
{
    public TMP_Text opponentHPText;
    public TMP_Text statusText;
    public Button shootButton;

    private string baseUrl;
    private int playerId;

    void Start()
    {
        baseUrl = "http://" + PlayerPrefs.GetString("ServerIP", "localhost") + ":3000";
        playerId = PlayerPrefs.GetInt("PlayerID", -1);

        // Удаляем проверку на -1, чтобы не блокировалась
        // Можно оставить лог для отладки
        if (playerId == -1)
        {
            Debug.LogWarning("Внимание: PlayerID не найден, но кнопка всё равно будет активна.");
            // Можно вообще удалить этот return, чтобы не мешал
        }

        // Разрешаем стрелять сразу
        shootButton.interactable = true;

        // Добавляем обработчик нажатия кнопки
        shootButton.onClick.AddListener(OnShootClicked);

        // Запускаем отслеживание состояния противника
        StartCoroutine(UpdateOpponentHP());
    }

    void OnShootClicked()
    {
        int damage = Random.Range(5, 20);
        shootButton.interactable = false;
        statusText.text = $"Вы стрельнули! Урон: {damage}";

        StartCoroutine(SendMove(damage));
    }

    IEnumerator SendMove(int damage)
    {
        WWWForm form = new WWWForm();
        form.AddField("playerId", playerId);
        form.AddField("damage", damage);

        UnityWebRequest request = UnityWebRequest.Post($"{baseUrl}/move", form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            statusText.text = "Урон нанесён!";
        }
        else
        {
            statusText.text = "Ошибка отправки урона.";
        }

        shootButton.interactable = true;
    }

    IEnumerator UpdateOpponentHP()
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/state");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                GameState state = JsonUtility.FromJson<GameState>(json);

                int opponentId = (playerId + 1) % 2;
                if (state.players.Length > opponentId)
                {
                    int enemyHP = state.players[opponentId].hp;
                    opponentHPText.text = $"Противник HP: {enemyHP}";
                }
            }

            yield return new WaitForSeconds(2);
        }
    }

    [System.Serializable]
    public class GameState
    {
        public Player[] players;
    }

    [System.Serializable]
    public class Player
    {
        public int id;
        public string name;
        public int hp;
    }
}
