using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    public ScrollRect scrollRect;
    public TMP_Text chatText;
    public TMP_InputField inputField;
    public Button sendButton;

    void Start()
    {
        inputField.onSubmit.AddListener(OnMessageSubmit);
        sendButton.onClick.AddListener(OnSendButtonClick);
    }

    void OnSendButtonClick()
    {
        string message = inputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            StartCoroutine(SendMessageToServer(message));
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    void OnMessageSubmit(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        StartCoroutine(SendMessageToServer(message));
        inputField.text = "";
        inputField.ActivateInputField();
    }

    public void AddMessage(string message)
    {
        chatText.text += "\n" + message;
        StartCoroutine(ScrollToBottom());
    }


    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    IEnumerator SendMessageToServer(string message)
    {
        string baseUrl = "http://" + PlayerPrefs.GetString("ServerIP", "localhost") + ":3000";
        int playerId = PlayerPrefs.GetInt("PlayerID", -1);

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
        if (inputField != null)
        {
            inputField.onSubmit.RemoveListener(OnMessageSubmit);
        }
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnSendButtonClick);
        }
    }
}