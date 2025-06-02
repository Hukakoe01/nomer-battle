using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

public class select_mode : MonoBehaviour
{
    [Header("������")]
    public Button storyModeButton;
    public Button endlessModeButton;
    public Button hostGameButton;
    public Button joinGameButton;
    public Button backButton;
    public Button exitButton;

    [Header("���� �����")]
    public TMP_InputField playerNameInput;

    [Header("������������ IP")]
    public IPScanner ipScanner;

    private const string MULTIPLAYER_SCENE = "MultiplayerGameScene";
    private const string SINGLEPLAYER_SCENE = "TutorialScene";

    void Start()
    {
        PlayerPrefs.SetString("PlayerName", GetPlayerName());
        storyModeButton.onClick.AddListener(OnStoryModeClicked);
        endlessModeButton.onClick.AddListener(OnEndlessModeClicked);
        hostGameButton.onClick.AddListener(OnHostGameClicked);
        joinGameButton.onClick.AddListener(OnJoinGameClicked);
        backButton.onClick.AddListener(OnBackClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    void OnStoryModeClicked()
    {
        PlayerPrefs.SetInt("GameMode", 0);
        SceneManager.LoadScene(SINGLEPLAYER_SCENE);
    }

    void OnEndlessModeClicked()
    {
        PlayerPrefs.SetInt("GameMode", 0);
        SceneManager.LoadScene(SINGLEPLAYER_SCENE);
    }

    void OnHostGameClicked()
    {
        PlayerPrefs.SetInt("GameMode", 1);
        PlayerSession.playerType = "host";
        PlayerPrefs.SetString("PlayerType", "host");
        PlayerSession.playerName = GetPlayerName();
        PlayerPrefs.SetString("PlayerName", PlayerSession.playerName);
        PlayerPrefs.SetString("ServerIP", "localhost");

        SceneManager.LoadScene("MultiplayerLobbyScene");
    }

    void OnJoinGameClicked()
    {
        PlayerPrefs.SetInt("GameMode", 1); 
        PlayerSession.playerType = "guest";
        PlayerPrefs.SetString("PlayerType", "guest");
        PlayerSession.playerName = GetPlayerName();
        PlayerPrefs.SetString("PlayerName", PlayerSession.playerName);

        Debug.Log("����� �������...");

        ipScanner.OnServerFound = (foundIP) =>
        {
            Debug.Log("������ ������ �� IP: " + foundIP);
            PlayerPrefs.SetString("ServerIP", foundIP);

            StartCoroutine(SendJoinRequest(foundIP));
        };

        ipScanner.StartScan();
    }

    IEnumerator SendJoinRequest(string ip)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        string joinUrl = $"http://{ip}:3000/join";

        WWWForm form = new WWWForm();
        form.AddField("playerName", playerName);

        Debug.Log($"[JOIN] ���������� ������: {joinUrl} | ���: {playerName}");

        UnityWebRequest req = UnityWebRequest.Post(joinUrl, form);
        yield return req.SendWebRequest();

        // ��������� ����������� ������ �������
        if (req.result != UnityWebRequest.Result.Success || req.responseCode != 200)
        {
            byte[] raw = req.downloadHandler.data;
            string json = System.Text.Encoding.UTF8.GetString(raw);
            Debug.Log("[JOIN] ����� �� ������� � �������: " + json);

            ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(json);
            if (error != null)
            {
                switch (error.error)
                {
                    case "invalid_name":
                        Debug.LogError("��� ������ �����������.");
                        break;
                    case "room_full":
                        Debug.LogError("������� ��� ���������.");
                        break;
                    case "already_joined":
                        Debug.LogError("����� � ����� ������ ��� � ����.");
                        break;
                    default:
                        Debug.LogError("����������� ������ ��� �����������.");
                        break;
                }
            }

            yield break; // �������� ����������, �� ������� �����!
        }

        // �������� �����
        string jsonResponse = req.downloadHandler.text;
        Debug.Log("[JOIN] �������� �����: " + jsonResponse);

        PlayerIdResponse response = JsonUtility.FromJson<PlayerIdResponse>(jsonResponse);
        PlayerPrefs.SetInt("PlayerID", response.playerId);

        SceneManager.LoadScene(MULTIPLAYER_SCENE);
    }

    void OnBackClicked()
    {
        SceneManager.LoadScene(0);
    }

    void OnExitClicked()
    {
        Application.Quit();
    }

    string GetPlayerName()
    {
        string name = playerNameInput != null ? playerNameInput.text.Trim() : "";
        return string.IsNullOrEmpty(name) ? "Player" : name;
    }

    // ��������������� ������
    [System.Serializable]
    public class PlayerIdResponse
    {
        public int playerId;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string error;
    }
}