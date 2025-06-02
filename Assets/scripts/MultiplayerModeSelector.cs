using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class MultiplayerModeSelector : MonoBehaviour
{
    public IPScanner ipScanner;
    public TMP_InputField guestIPInput; // поле, где гость может ввести IP
    public string multiplayerLobbyScene = "MultiplayerLobby"; // куда потом перейти

    public void OnHostClicked()
    {
        PlayerSession.playerType = "host";
        PlayerPrefs.SetString("ServerIP", "localhost"); // запускаем сервер локально
        SceneManager.LoadScene(multiplayerLobbyScene); // иди в сцену меню подключения
    }

    public void OnGuestClicked()
    {
        string ip = guestIPInput.text;

        if (string.IsNullOrWhiteSpace(ip))
        {
            Debug.Log("IP не введён — ищем сервер в локальной сети...");

            ipScanner.OnServerFound = (foundIP) =>
            {
                // Можно показать найденный IP в консоли
                Debug.Log("Найден сервер на IP: " + foundIP);

                // Устанавливаем сетевые настройки
                PlayerPrefs.SetString("ServerIP", foundIP);
                PlayerSession.playerType = "guest";

                // Переход в следующую сцену
                SceneManager.LoadScene(multiplayerLobbyScene);
            };

            ipScanner.StartScan(); //  запускаем скан
            return;
        }

        // Если IP был введён вручную
        PlayerSession.playerType = "guest";
        PlayerPrefs.SetString("ServerIP", ip);
        SceneManager.LoadScene(multiplayerLobbyScene);
    }
}