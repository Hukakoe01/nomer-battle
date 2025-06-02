using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics; //  нужен для Process
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class ServerLauncher : MonoBehaviour
{
    private Process serverProcess;


    void Start()
    {
        // Определяем тип игрока в зависимости от текущей сцены
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "selectmode")
        {
            PlayerSession.playerType = "guest";
            PlayerPrefs.SetString("PlayerType", "guest");
            UnityEngine.Debug.Log("[SERVER LAUNCHER] Сцена selectmode - устанавливаем тип игрока: гость");
        }
        else if (currentScene == "MultiplayerLobbyScene")
        {
            PlayerSession.playerType = "host";
            PlayerPrefs.SetString("PlayerType", "host");
            UnityEngine.Debug.Log("[SERVER LAUNCHER] Сцена MultiplayerLobbyScene - устанавливаем тип игрока: хост");
        }

        if (PlayerSession.playerType == "host")
        {
            UnityEngine.Debug.Log("[SERVER LAUNCHER] Хост — запускаем сервер.");
            StartCoroutine(CheckServerRunning(
                onAlreadyRunning: () =>
                {
                    UnityEngine.Debug.LogWarning("Node.js уже работает.");
                },
                onNotRunning: () =>
                {
                    UnityEngine.Debug.Log("[SERVER LAUNCHER] Сервер не найден. Запускаем...");
                    StartServer();
                }));
        }
        else
        {
            UnityEngine.Debug.Log("[SERVER LAUNCHER] Гость — сервер запускать не нужно.");
        }
    }

    private IEnumerator CheckServerRunning(System.Action onAlreadyRunning, System.Action onNotRunning)
    {
        UnityWebRequest req = UnityWebRequest.Get("http://localhost:3000/state");
        req.timeout = 1;

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            onAlreadyRunning?.Invoke();
        }
        else
        {
            onNotRunning?.Invoke();
        }
    }

    private void StartServer()
    {
        string nodePath = Path.Combine(Application.streamingAssetsPath, "node.exe");
        string serverScriptPath = Path.Combine(Application.streamingAssetsPath, "server.js");

        if (!File.Exists(nodePath))
        {
            UnityEngine.Debug.LogError("Не найден node.exe: " + nodePath);
            return;
        }

        if (!File.Exists(serverScriptPath))
        {
            UnityEngine.Debug.LogError("Не найден server.js: " + serverScriptPath);
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = nodePath,
            Arguments = $"\"{serverScriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Application.streamingAssetsPath
        };

        serverProcess = new Process { StartInfo = startInfo };

        serverProcess.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.Log("[SERVER] " + args.Data);
        };

        serverProcess.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.LogError("[SERVER ERROR] " + args.Data);
        };

        try
        {
            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();
            UnityEngine.Debug.Log("[SERVER LAUNCHER] Сервер успешно запущен.");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Ошибка при запуске сервера: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();     // Главное - убиваем
            serverProcess.Dispose();  // Чистим за собой
            serverProcess = null;

            UnityEngine.Debug.Log("[SERVER LAUNCHER] Node.js сервер завершён при выходе.");
        }
    }
}