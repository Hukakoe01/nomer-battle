using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using UnityEngine.Networking;
using System.Diagnostics;

public class main_menu : MonoBehaviour
{
    public GameObject Panel;
    public GameObject Button1;
    public GameObject Button2;
    public GameObject Button_exite;

    private string dbPath;

    public void buton_play()
    {
        SceneManager.LoadScene("selectmode");
    }

    public void buton_setting()
    {
        Panel.SetActive(true);
        Button1.gameObject.SetActive(true);
        Button2.gameObject.SetActive(true);
    }

    public void Close_setting()
    {
        Panel.SetActive(false);
        Button1.gameObject.SetActive(false);
        Button2.gameObject.SetActive(false);
    }

    void Start()
    {
        UnityEngine.Debug.Log("Persistent Data Path: " + Application.persistentDataPath);

        KillAllNodeProcesses();

        string fileName = "data.db";
        string targetPath = Path.Combine(Application.persistentDataPath, fileName);
        string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);

        dbPath = "URI=file:" + targetPath;

        if (!File.Exists(targetPath))
        {
            UnityEngine.Debug.Log("Копируем базу в persistentDataPath...");
        }
        else
        {
            UnityEngine.Debug.Log("База уже существует, копировать не нужно.");
        }
    }

    public void button_exit()
    {
        UnityEngine.Debug.Log("Игра закрывается...");
        Application.Quit();
    }

    void KillAllNodeProcesses()
    {
        Process.Start("taskkill", "/F /IM node.exe");
    }

    IEnumerator StopServer()
    {
        UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:3000/shutdown", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("{}");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        UnityEngine.Debug.Log("Ответ на запрос:");
        UnityEngine.Debug.Log("Код ответа: " + request.responseCode);
        UnityEngine.Debug.Log("Текст: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
            UnityEngine.Debug.Log("Сервер успешно остановлен.");
        else
            UnityEngine.Debug.LogError("Не удалось остановить сервер: " + request.error);
    }
}