using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class MultiplayerModeSelector : MonoBehaviour
{
    public IPScanner ipScanner;
    public TMP_InputField guestIPInput; // ����, ��� ����� ����� ������ IP
    public string multiplayerLobbyScene = "MultiplayerLobby"; // ���� ����� �������

    public void OnHostClicked()
    {
        PlayerSession.playerType = "host";
        PlayerPrefs.SetString("ServerIP", "localhost"); // ��������� ������ ��������
        SceneManager.LoadScene(multiplayerLobbyScene); // ��� � ����� ���� �����������
    }

    public void OnGuestClicked()
    {
        string ip = guestIPInput.text;

        if (string.IsNullOrWhiteSpace(ip))
        {
            Debug.Log("IP �� ����� � ���� ������ � ��������� ����...");

            ipScanner.OnServerFound = (foundIP) =>
            {
                // ����� �������� ��������� IP � �������
                Debug.Log("������ ������ �� IP: " + foundIP);

                // ������������� ������� ���������
                PlayerPrefs.SetString("ServerIP", foundIP);
                PlayerSession.playerType = "guest";

                // ������� � ��������� �����
                SceneManager.LoadScene(multiplayerLobbyScene);
            };

            ipScanner.StartScan(); //  ��������� ����
            return;
        }

        // ���� IP ��� ����� �������
        PlayerSession.playerType = "guest";
        PlayerPrefs.SetString("ServerIP", ip);
        SceneManager.LoadScene(multiplayerLobbyScene);
    }
}