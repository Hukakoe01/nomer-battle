using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics; //  ����� ��� Process
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class ServerLauncher : MonoBehaviour
{
    private Process serverProcess;


    void Start()
    {
        // ���������� ��� ������ � ����������� �� ������� �����
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "selectmode")
        {
            PlayerSession.playerType = "guest";
            PlayerPrefs.SetString("PlayerType", "guest");
            UnityEngine.Debug.Log("[SERVER LAUNCHER] ����� selectmode - ������������� ��� ������: �����");
        }
        else if (currentScene == "MultiplayerLobbyScene")
        {
            PlayerSession.playerType = "host";
            PlayerPrefs.SetString("PlayerType", "host");
            UnityEngine.Debug.Log("[SERVER LAUNCHER] ����� MultiplayerLobbyScene - ������������� ��� ������: ����");
        }

        if (PlayerSession.playerType == "host")
        {
            UnityEngine.Debug.Log("[SERVER LAUNCHER] ���� � ��������� ������.");
            StartCoroutine(CheckServerRunning(
                onAlreadyRunning: () =>
                {
                    UnityEngine.Debug.LogWarning("Node.js ��� ��������.");
                },
                onNotRunning: () =>
                {
                    UnityEngine.Debug.Log("[SERVER LAUNCHER] ������ �� ������. ���������...");
                    StartServer();
                }));
        }
        else
        {
            UnityEngine.Debug.Log("[SERVER LAUNCHER] ����� � ������ ��������� �� �����.");
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
            UnityEngine.Debug.LogError("�� ������ node.exe: " + nodePath);
            return;
        }

        if (!File.Exists(serverScriptPath))
        {
            UnityEngine.Debug.LogError("�� ������ server.js: " + serverScriptPath);
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
            UnityEngine.Debug.Log("[SERVER LAUNCHER] ������ ������� �������.");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("������ ��� ������� �������: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();     // ������� - �������
            serverProcess.Dispose();  // ������ �� �����
            serverProcess = null;

            UnityEngine.Debug.Log("[SERVER LAUNCHER] Node.js ������ �������� ��� ������.");
        }
    }
}